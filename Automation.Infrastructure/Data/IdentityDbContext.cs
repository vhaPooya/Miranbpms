using Automation.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Automation.Infrastructure.Data;

/// <summary>
/// دامنه استاتیک/ادمین — Users, Roles, Permissions, Organization, Settings (EF Core Code-First).
/// طبق معماری Hybrid جدا از دامنه داینامیک (FormBuilder/Workflow با Dapper+SP).
/// </summary>
public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Position> Positions { get; set; } = null!;
    public DbSet<UserPosition> UserPositions { get; set; } = null!;
    public DbSet<RolePosition> RolePositions { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<UserSignature> UserSignatures { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<SystemSetting> SystemSettings { get; set; } = null!;
    public DbSet<PermissionGroup> PermissionGroups { get; set; } = null!;
    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<GroupMember> GroupMembers { get; set; } = null!;
    public DbSet<GroupPermission> GroupPermissions { get; set; } = null!;
    public DbSet<RoleGroup> RoleGroups { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureUser(modelBuilder);
        ConfigureRole(modelBuilder);
        ConfigurePosition(modelBuilder);
        ConfigureUserPosition(modelBuilder);
        ConfigureRolePosition(modelBuilder);
        ConfigureUserRole(modelBuilder);
        ConfigureUserSignature(modelBuilder);
        ConfigurePermission(modelBuilder);
        ConfigureRolePermission(modelBuilder);
        ConfigureDepartment(modelBuilder);
        ConfigureOrganization(modelBuilder);
        ConfigureSystemSetting(modelBuilder);
        ConfigurePermissionGroup(modelBuilder);
        ConfigureGroup(modelBuilder);
        ConfigureGroupMember(modelBuilder);
        ConfigureGroupPermission(modelBuilder);
        ConfigureRoleGroup(modelBuilder);

        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Role>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Position>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserPosition>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RolePosition>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserRole>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserSignature>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Permission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RolePermission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Department>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Organization>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Group>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<GroupMember>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<GroupPermission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PermissionGroup>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RoleGroup>().HasQueryFilter(rg => !rg.Group.IsDeleted);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Mobile).HasMaxLength(20);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasOne(e => e.Department).WithMany(d => d.Users).HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Organization).WithMany(o => o.Users).HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RoleName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.RoleCode).IsUnique();
            entity.HasOne(e => e.ParentRole).WithMany(r => r.ChildRoles).HasForeignKey(e => e.ParentRoleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Organization).WithMany().HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.PersonnelUser).WithMany().HasForeignKey(e => e.PersonnelUserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Department).WithMany(d => d.Roles).HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.CorrespondenceTitle).HasMaxLength(200);
            entity.Property(e => e.DisplayTitle).HasMaxLength(200);
        });
    }

    private static void ConfigurePosition(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Position>(entity =>
        {
            entity.ToTable("Positions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PositionCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PositionName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.PositionCode).IsUnique();
            entity.HasOne(e => e.ParentPosition).WithMany(p => p.ChildPositions).HasForeignKey(e => e.ParentPositionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Organization).WithMany().HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Department).WithMany().HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureUserPosition(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserPosition>(entity =>
        {
            entity.ToTable("UserPositions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.PositionId }).IsUnique();
            entity.HasOne(e => e.User).WithMany(u => u.UserPositions).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Position).WithMany(p => p.UserPositions).HasForeignKey(e => e.PositionId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureRolePosition(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolePosition>(entity =>
        {
            entity.ToTable("RolePositions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RoleId, e.PositionId }).IsUnique();
            entity.HasOne(e => e.Role).WithMany(r => r.RolePositions).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Position).WithMany(p => p.RolePositions).HasForeignKey(e => e.PositionId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureUserRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
            entity.HasOne(e => e.User).WithMany(u => u.UserRoles).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Role).WithMany(r => r.UserRoles).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureUserSignature(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSignature>(entity =>
        {
            entity.ToTable("UserSignatures");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SignatureTitle).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SignaturePath).HasMaxLength(1000).IsRequired();
            entity.HasOne(e => e.User).WithMany(u => u.UserSignatures).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.IsDefault });
        });
    }

    private static void ConfigurePermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PermissionCode).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PermissionName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.HasIndex(e => e.PermissionCode).IsUnique();
            entity.HasOne(e => e.PermissionGroup).WithMany(g => g.Permissions).HasForeignKey(e => e.PermissionGroupId).OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureRolePermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
            entity.HasOne(e => e.Role).WithMany(r => r.RolePermissions).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Permission).WithMany(p => p.RolePermissions).HasForeignKey(e => e.PermissionId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureDepartment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Departments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DepartmentCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DepartmentName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.DepartmentCode).IsUnique();
            entity.HasOne(e => e.Organization).WithMany(o => o.Departments).HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.ParentDepartment).WithMany(d => d.ChildDepartments).HasForeignKey(e => e.ParentDepartmentId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureOrganization(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("Organizations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrganizationCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.OrganizationName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.OrganizationCode).IsUnique();
        });
    }

    private static void ConfigureSystemSetting(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.ToTable("SystemSettings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SettingKey).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SettingValue).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.SettingType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.SettingKey).IsUnique();
        });
    }

    private static void ConfigurePermissionGroup(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PermissionGroup>(entity =>
        {
            entity.ToTable("PermissionGroups");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GroupCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.GroupName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Icon).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.GroupCode).IsUnique();
            entity.HasOne(e => e.ParentGroup).WithMany(g => g.ChildGroups).HasForeignKey(e => e.ParentGroupId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureGroup(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Group>(entity =>
        {
            entity.ToTable("Groups");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GroupCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.GroupName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.GroupCode).IsUnique();
            entity.HasOne(e => e.ParentGroup).WithMany(g => g.ChildGroups).HasForeignKey(e => e.ParentGroupId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureGroupMember(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.ToTable("GroupMembers");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Group).WithMany(g => g.Members).HasForeignKey(e => e.GroupId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasIndex(e => new { e.GroupId, e.UserId }).IsUnique();
        });
    }

    private static void ConfigureGroupPermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GroupPermission>(entity =>
        {
            entity.ToTable("GroupPermissions");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Group).WithMany(g => g.GroupPermissions).HasForeignKey(e => e.GroupId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Permission).WithMany(p => p.GroupPermissions).HasForeignKey(e => e.PermissionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.GroupId, e.PermissionId }).IsUnique();
        });
    }

    private static void ConfigureRoleGroup(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoleGroup>(entity =>
        {
            entity.ToTable("RoleGroups");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Role).WithMany(r => r.RoleGroups).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Group).WithMany(g => g.RoleGroups).HasForeignKey(e => e.GroupId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.RoleId, e.GroupId }).IsUnique();
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified)))
        {
            var entity = (BaseEntity)entry.Entity;
            if (entry.State == EntityState.Added) { entity.CreationDate = DateTime.UtcNow; entity.CreatedAt = DateTime.UtcNow; }
            if (entry.State == EntityState.Modified) { entity.EditDate = DateTime.UtcNow; entity.UpdatedAt = DateTime.UtcNow; }
        }
    }
}
