using Automation.Core.Entities;
using Automation.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Automation.Infrastructure.Data;

/// <summary>
/// Seeder for Super Admin user with full permissions
/// </summary>
public static class SuperAdminSeeder
{
    public static async Task SeedSuperAdminAsync(IdentityDbContext context, IPasswordHasher passwordHasher)
    {
        // Check if super admin already exists
        var existingSuperAdmin = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Username == "superadmin" && !u.IsDeleted);

        if (existingSuperAdmin != null)
            return;

        // Create organization if not exists
        var organization = await context.Organizations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.OrganizationCode == "MAIN_ORG");

        if (organization == null)
        {
            organization = new Organization
            {
                OrganizationCode = "MAIN_ORG",
                OrganizationName = "سازمان اصلی",
                Description = "سازمان اصلی سیستم"
            };
            context.Organizations.Add(organization);
            await context.SaveChangesAsync();
        }

        // Create department if not exists
        var department = await context.Departments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.DepartmentCode == "IT_DEPT");

        if (department == null)
        {
            department = new Department
            {
                DepartmentCode = "IT_DEPT",
                DepartmentName = "واحد فناوری اطلاعات",
                OrganizationId = organization.Id,
                Description = "واحد فناوری اطلاعات و پشتیبانی سیستم"
            };
            context.Departments.Add(department);
            await context.SaveChangesAsync();
        }

        // Create super admin role if not exists
        var superAdminRole = await context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.RoleCode == "SUPER_ADMIN");

        if (superAdminRole == null)
        {
            superAdminRole = new Role
            {
                RoleCode = "SUPER_ADMIN",
                RoleName = "مدیر کل سیستم",
                Description = "نقش مدیر کل با دسترسی کامل به تمام بخش‌های سیستم",
                OrganizationId = organization.Id
            };
            context.Roles.Add(superAdminRole);
            await context.SaveChangesAsync();
        }

        // Create super admin position if not exists
        var superAdminPosition = await context.Positions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.PositionCode == "SUPER_ADMIN_POS");

        if (superAdminPosition == null)
        {
            superAdminPosition = new Position
            {
                PositionCode = "SUPER_ADMIN_POS",
                PositionName = "مدیر کل سیستم",
                Description = "سمت مدیر کل با دسترسی کامل به تمام بخش‌های سیستم",
                OrganizationId = organization.Id,
                DepartmentId = department.Id
            };
            context.Positions.Add(superAdminPosition);
            await context.SaveChangesAsync();
        }

        // Create super admin user
        var superAdminUser = new User
        {
            Username = "superadmin",
            PasswordHash = passwordHasher.HashPassword("Admin@123456"),
            FirstName = "مدیر",
            LastName = "کل",
            Email = "admin@organization.com",
            Mobile = "09120000000",
            OrganizationId = organization.Id,
            DepartmentId = department.Id,
            IsActive = true,
            IsDeleted = false
        };

        context.Users.Add(superAdminUser);
        await context.SaveChangesAsync();

        // Assign user to position
        var userPosition = new UserPosition
        {
            UserId = superAdminUser.Id,
            PositionId = superAdminPosition.Id,
            IsActive = true
        };
        context.UserPositions.Add(userPosition);

        // Assign role to user
        var userRole = new UserRole
        {
            UserId = superAdminUser.Id,
            RoleId = superAdminRole.Id,
            IsActive = true
        };
        context.UserRoles.Add(userRole);

        await context.SaveChangesAsync();

        // Assign all permissions to super admin role
        var allPermissions = await context.Permissions
            .IgnoreQueryFilters()
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        var rolePermissions = allPermissions.Select(permission => new RolePermission
        {
            RoleId = superAdminRole.Id,
            PermissionId = permission.Id,
            IsActive = true
        }).ToList();

        await context.RolePermissions.AddRangeAsync(rolePermissions);
        await context.SaveChangesAsync();

        Console.WriteLine("Super admin user created successfully!");
        Console.WriteLine("Username: superadmin");
        Console.WriteLine("Password: Admin@123456");
        Console.WriteLine("Please change the password after first login.");
    }
}