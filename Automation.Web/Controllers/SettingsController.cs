using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Automation.Web.Controllers;

/// <summary>
/// Settings Controller for managing Users, Roles, Organizations, Departments, etc.
/// Uses IdentityDbContext for identity data; AutomationDbContext only for Documents/Forms (e.g. used paths).
/// </summary>
public class SettingsController : Controller
{
    private readonly IdentityDbContext _identityContext;
    private readonly AutomationDbContext _context;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(IdentityDbContext identityContext, AutomationDbContext context, ILogger<SettingsController> logger)
    {
        _identityContext = identityContext;
        _context = context;
        _logger = logger;
    }


    /// <summary>
    /// صفحه اصلی تنظیمات (سایدبار + container)
    /// </summary>
    [HttpGet]
    [Route("settingspage")]
    public IActionResult Index()
    {
        return PartialView("Index");
    }

    /// <summary>
    /// بارگذاری Partial View برای هر بخش تنظیمات
    /// </summary>
    [HttpGet]
    [Route("Settings/Partial/{section}")]
    public IActionResult Partial(string section)
    {
        switch (section)
        {
            case "general-settings": return PartialView("_GeneralSettings");
            case "users":            return PartialView("_Users");
            case "positions":        return PartialView("_Positions");
            case "hr-roles":         return PartialView("_HrRoles");
            case "user-groups":      return PartialView("_UserGroups");
            case "departments":      return PartialView("_Departments");
            case "organizations":    return PartialView("_Organizations");

            case "general-permissions":
            case "secretariat-permissions":
            case "formbuilder-permissions":
            case "report-permissions":
                ViewBag.ActiveCategory = section.Replace("-permissions", "");
                return PartialView("_Permissions");

            case "email-settings":     return PartialView("_Placeholder", GetPlaceholderModel("تنظیمات ایمیل", "fa-solid fa-envelope"));
            case "fax-settings":       return PartialView("_Placeholder", GetPlaceholderModel("تنظیمات فکس", "fa-solid fa-fax"));
            case "integrations":       return PartialView("_Placeholder", GetPlaceholderModel("ارتباط با سایر سامانه ها", "fa-solid fa-plug"));
            case "increase-access":    return PartialView("_Placeholder", GetPlaceholderModel("افزایش سطح دسترسی", "fa-solid fa-arrow-up"));
            case "decrease-access":    return PartialView("_Placeholder", GetPlaceholderModel("کاهش سطح دسترسی", "fa-solid fa-arrow-down"));
            case "login-settings":     return PartialView("_Placeholder", GetPlaceholderModel("تنظیمات صفحه ورود", "fa-solid fa-right-to-bracket"));
            case "lock-info":          return PartialView("_Placeholder", GetPlaceholderModel("اطلاعات قفل", "fa-solid fa-lock"));
            case "software-info":      return PartialView("_Placeholder", GetPlaceholderModel("اطلاعات نرم افزار", "fa-solid fa-circle-info"));
            default:                   return PartialView("_Placeholder", GetPlaceholderModel("بخش ناشناخته", "fa-solid fa-question"));
        }
    }

    private dynamic GetPlaceholderModel(string title, string icon)
    {
        ViewBag.Title = title;
        ViewBag.Icon = icon;
        return null;
    }

    // ==========================================
    //           USERS MANAGEMENT
    // ==========================================

    [HttpGet]
    [Route("api/settings/users")]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? nationalId = null, [FromQuery] string? status = null)
    {
        var query = _identityContext.Users
            .Where(u => !u.IsDeleted)
            .Include(u => u.Department)
            .Include(u => u.Organization)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u =>
                u.Username.Contains(search) ||
                u.FirstName.Contains(search) ||
                u.LastName.Contains(search) ||
                (u.Email != null && u.Email.Contains(search)));
        }

        if (!string.IsNullOrEmpty(nationalId))
            query = query.Where(u => u.NationalId != null && u.NationalId.Contains(nationalId));

        if (status == "active")
            query = query.Where(u => u.IsActive);
        else if (status == "inactive")
            query = query.Where(u => !u.IsActive);

        var totalCount = await query.CountAsync();
        var users = await query
            .Include(u => u.UserRoles.Where(ur => !ur.IsDeleted))
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserSignatures.Where(s => !s.IsDeleted))
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.Title,
                u.Username,
                u.FirstName,
                u.LastName,
                u.FatherName,
                u.NationalId,
                u.Gender,
                u.MaritalStatus,
                u.PersonnelNumber,
                u.Email,
                u.Mobile,
                u.Phone,
                u.Address,
                u.City,
                u.Province,
                u.Notes,
                u.AvatarPath,
                u.IsOnline,
                u.LastLoginDate,
                DepartmentName = u.Department != null ? u.Department.DepartmentName : null,
                OrganizationName = u.Organization != null ? u.Organization.OrganizationName : null,
                u.DepartmentId,
                u.OrganizationId,
                u.IsActive,
                u.CreationDate,
                u.CreatedAt,
                Roles = u.UserRoles.Select(ur => new
                {
                    ur.RoleId,
                    RoleName = ur.Role != null ? ur.Role.RoleName : null
                }).ToList(),
                Signatures = u.UserSignatures.Select(s => new
                {
                    s.Id,
                    s.SignatureTitle,
                    s.SignaturePath,
                    s.IsDefault
                }).ToList()
            })
            .ToListAsync();

        return Json(new { success = true, data = users, totalCount, page, pageSize });
    }

    [HttpGet]
    [Route("api/settings/users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _identityContext.Users
            .Include(u => u.Department)
            .Include(u => u.Organization)
            .Include(u => u.UserRoles.Where(ur => !ur.IsDeleted))
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserSignatures.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null)
            return NotFound();

        // Check if signatures are used in documents
        // Note: This assumes there's a SignaturePath field in Documents table
        // If your schema is different, adjust this query accordingly
        var usedSignaturePaths = new HashSet<string>();
        try
        {
            var usedPaths = await _context.Documents
                .Where(d => !d.IsDeleted && d.SignedByUserId == id && !string.IsNullOrEmpty(d.SignaturePath))
                .Select(d => d.SignaturePath!)
                .Distinct()
                .ToListAsync();

            usedSignaturePaths = usedPaths.ToHashSet();
        }
        catch
        {
            // If Documents table doesn't have SignaturePath field, ignore
            // This prevents errors if the field doesn't exist yet
        }

        return Json(new
        {
            success = true,
            data = new
            {
                user.Id,
                user.Title,
                user.Username,
                user.FirstName,
                user.LastName,
                user.FatherName,
                user.NationalId,
                user.Gender,
                user.MaritalStatus,
                user.PersonnelNumber,
                user.Email,
                user.Mobile,
                user.Phone,
                user.Address,
                user.City,
                user.Province,
                user.Notes,
                user.AvatarPath,
                user.IsOnline,
                user.LastLoginDate,
                user.DepartmentId,
                user.OrganizationId,
                user.IsActive,
                DepartmentName = user.Department?.DepartmentName,
                OrganizationName = user.Organization?.OrganizationName,
                Roles = user.UserRoles.Select(ur => new
                {
                    ur.RoleId,
                    RoleName = ur.Role != null ? ur.Role.RoleName : null
                }).ToList(),
                Signatures = user.UserSignatures.Select(s => new
                {
                    s.Id,
                    s.SignatureTitle,
                    s.SignaturePath,
                    s.IsDefault,
                    IsUsed = usedSignaturePaths.Contains(s.SignaturePath)
                }).ToList()
            }
        });
    }

    [HttpPost]
    [Route("api/settings/users")]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Check if username exists
        if (await _identityContext.Users.AnyAsync(u => u.Username == dto.Username && !u.IsDeleted))
            return BadRequest(new { error = "نام کاربری تکراری است" });

        // Check if email exists
        if (!string.IsNullOrEmpty(dto.Email) && await _identityContext.Users.AnyAsync(u => u.Email == dto.Email && !u.IsDeleted))
            return BadRequest(new { error = "ایمیل تکراری است" });

        // Check if national ID exists
        if (!string.IsNullOrEmpty(dto.NationalId) && await _identityContext.Users.AnyAsync(u => u.NationalId == dto.NationalId && !u.IsDeleted))
            return BadRequest(new { error = "کد ملی تکراری است" });

        var user = new User
        {
            Title = dto.Title,
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), // Using BCrypt for password hashing
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            FatherName = dto.FatherName,
            NationalId = dto.NationalId,
            Gender = dto.Gender,
            MaritalStatus = dto.MaritalStatus,
            PersonnelNumber = dto.PersonnelNumber,
            Email = dto.Email,
            Mobile = dto.Mobile,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            Province = dto.Province,
            Notes = dto.Notes,
            AvatarPath = dto.AvatarPath,
            DepartmentId = dto.DepartmentId,
            OrganizationId = dto.OrganizationId,
            IsActive = dto.IsActive ?? true
        };

        _identityContext.Users.Add(user);
        await _identityContext.SaveChangesAsync();

        // Add roles if provided
        if (dto.RoleIds != null && dto.RoleIds.Any())
        {
            foreach (var roleId in dto.RoleIds)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleId
                };
                _identityContext.UserRoles.Add(userRole);
            }
            await _identityContext.SaveChangesAsync();
        }

        // Add signatures if provided
        if (dto.Signatures != null && dto.Signatures.Any())
        {
            foreach (var signatureDto in dto.Signatures)
            {
                var signature = new UserSignature
                {
                    UserId = user.Id,
                    SignatureTitle = signatureDto.SignatureTitle,
                    SignaturePath = signatureDto.SignaturePath,
                    IsDefault = signatureDto.IsDefault
                };
                _identityContext.Set<UserSignature>().Add(signature);
            }
            await _identityContext.SaveChangesAsync();
        }

        return Json(new { success = true, id = user.Id });
    }

    [HttpPut]
    [Route("api/settings/users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto dto)
    {
        var user = await _identityContext.Users.FindAsync(id);
        if (user == null || user.IsDeleted)
            return NotFound();

        // Check username uniqueness
        if (!string.IsNullOrEmpty(dto.Username) && dto.Username != user.Username)
        {
            if (await _identityContext.Users.AnyAsync(u => u.Username == dto.Username && u.Id != id && !u.IsDeleted))
                return BadRequest(new { error = "نام کاربری تکراری است" });
            user.Username = dto.Username;
        }

        // Check email uniqueness
        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
        {
            if (await _identityContext.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id && !u.IsDeleted))
                return BadRequest(new { error = "ایمیل تکراری است" });
            user.Email = dto.Email;
        }

        // Check national ID uniqueness
        if (!string.IsNullOrEmpty(dto.NationalId) && dto.NationalId != user.NationalId)
        {
            if (await _identityContext.Users.AnyAsync(u => u.NationalId == dto.NationalId && u.Id != id && !u.IsDeleted))
                return BadRequest(new { error = "کد ملی تکراری است" });
            user.NationalId = dto.NationalId;
        }

        if (dto.Title != null) user.Title = dto.Title;
        if (!string.IsNullOrEmpty(dto.FirstName)) user.FirstName = dto.FirstName;
        if (!string.IsNullOrEmpty(dto.LastName)) user.LastName = dto.LastName;
        if (dto.FatherName != null) user.FatherName = dto.FatherName;
        if (dto.Gender != null) user.Gender = dto.Gender;
        if (dto.MaritalStatus != null) user.MaritalStatus = dto.MaritalStatus;
        if (dto.PersonnelNumber != null) user.PersonnelNumber = dto.PersonnelNumber;
        if (dto.Mobile != null) user.Mobile = dto.Mobile;
        if (dto.Phone != null) user.Phone = dto.Phone;
        if (dto.Address != null) user.Address = dto.Address;
        if (dto.City != null) user.City = dto.City;
        if (dto.Province != null) user.Province = dto.Province;
        if (dto.Notes != null) user.Notes = dto.Notes;
        if (dto.AvatarPath != null) user.AvatarPath = dto.AvatarPath;
        if (dto.DepartmentId.HasValue) user.DepartmentId = dto.DepartmentId;
        if (dto.OrganizationId.HasValue) user.OrganizationId = dto.OrganizationId;
        if (dto.IsActive.HasValue) user.IsActive = dto.IsActive.Value;
        if (!string.IsNullOrEmpty(dto.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        user.EditDate = DateTime.UtcNow;
        await _identityContext.SaveChangesAsync();

        // Update roles if provided
        if (dto.RoleIds != null)
        {
            // Remove existing roles
            var existingRoles = await _identityContext.UserRoles
                .Where(ur => ur.UserId == id && !ur.IsDeleted)
                .ToListAsync();

            foreach (var role in existingRoles)
            {
                role.IsDeleted = true;
            }

            // Add new roles
            foreach (var roleId in dto.RoleIds)
            {
                var userRole = new UserRole
                {
                    UserId = id,
                    RoleId = roleId
                };
                _identityContext.UserRoles.Add(userRole);
            }
            await _identityContext.SaveChangesAsync();
        }

        // Update signatures if provided
        if (dto.Signatures != null)
        {
            // Remove existing signatures
            var existingSignatures = await _identityContext.Set<UserSignature>()
                .Where(s => s.UserId == id && !s.IsDeleted)
                .ToListAsync();

            foreach (var signature in existingSignatures)
            {
                signature.IsDeleted = true;
            }

            // Add new signatures
            foreach (var signatureDto in dto.Signatures)
            {
                var signature = new UserSignature
                {
                    UserId = id,
                    SignatureTitle = signatureDto.SignatureTitle,
                    SignaturePath = signatureDto.SignaturePath,
                    IsDefault = signatureDto.IsDefault
                };
                _identityContext.Set<UserSignature>().Add(signature);
            }
            await _identityContext.SaveChangesAsync();
        }

        return Json(new { success = true });
    }

    [HttpDelete]
    [Route("api/settings/users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _identityContext.Users.FindAsync(id);
        if (user == null || user.IsDeleted)
            return NotFound();

        user.IsDeleted = true;
        user.EditDate = DateTime.UtcNow;
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    /// <summary>
    /// Upload user avatar image
    /// </summary>
    [HttpPost]
    [Route("api/settings/users/upload-avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "فایلی انتخاب نشده است" });

            // Validate file size (max 2MB)
            if (file.Length > 2 * 1024 * 1024)
                return BadRequest(new { error = "حجم فایل نباید بیشتر از 2 مگابایت باشد" });

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { error = "فرمت فایل معتبر نیست. فقط فایل‌های jpg، jpeg، png و gif مجاز هستند" });

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path
            var relativePath = $"/uploads/avatars/{fileName}";
            return Json(new { success = true, path = relativePath });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading avatar");
            return StatusCode(500, new { error = "خطا در آپلود فایل" });
        }
    }

    /// <summary>
    /// Upload user signature image
    /// </summary>
    [HttpPost]
    [Route("api/settings/users/upload-signature")]
    public async Task<IActionResult> UploadSignature(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "فایلی انتخاب نشده است" });

            // Validate file size (max 2MB)
            if (file.Length > 2 * 1024 * 1024)
                return BadRequest(new { error = "حجم فایل نباید بیشتر از 2 مگابایت باشد" });

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { error = "فرمت فایل معتبر نیست. فقط فایل‌های jpg، jpeg، png و gif مجاز هستند" });

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "signatures");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path
            var relativePath = $"/uploads/signatures/{fileName}";
            return Json(new { success = true, path = relativePath });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading signature");
            return StatusCode(500, new { error = "خطا در آپلود فایل" });
        }
    }

    // ==========================================
    //           ROLES MANAGEMENT
    // ==========================================

    [HttpGet]
    [Route("settings/roles")]
    public IActionResult Roles()
    {
        return View();
    }

    [HttpGet]
    [Route("api/settings/roles")]
    public async Task<IActionResult> GetRoles([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var query = _identityContext.Roles
            .Where(r => !r.IsDeleted)
            .Include(r => r.ParentRole)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(r => 
                r.RoleCode.Contains(search) ||
                r.RoleName.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var roles = await query
            .OrderBy(r => r.RoleCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.RoleCode,
                r.RoleName,
                r.Description,
                ParentRoleName = r.ParentRole != null ? r.ParentRole.RoleName : null,
                r.ParentRoleId,
                r.IsActive,
                r.CreationDate
            })
            .ToListAsync();

        return Json(new { success = true, data = roles, totalCount, page, pageSize });
    }

    [HttpGet]
    [Route("api/settings/roles/{id}")]
    public async Task<IActionResult> GetRole(int id)
    {
        var role = await _identityContext.Roles
            .Include(r => r.ParentRole)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.RoleGroups.Where(rg => !rg.IsDeleted))
            .Include(r => r.PersonnelUser)
            .Include(r => r.Department)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (role == null)
            return NotFound();

        return Json(new
        {
            success = true,
            data = new
            {
                role.Id,
                role.RoleCode,
                role.RoleName,
                role.Description,
                role.ParentRoleId,
                role.OrganizationId,
                role.IsActive,
                role.CorrespondenceTitle,
                role.DisplayTitle,
                role.PersonnelUserId,
                PersonnelUserName = role.PersonnelUser != null ? $"{role.PersonnelUser.FirstName} {role.PersonnelUser.LastName}" : null,
                role.DepartmentId,
                DepartmentName = role.Department?.DepartmentName,
                Permissions = role.RolePermissions.Where(rp => !rp.IsDeleted).Select(rp => rp.PermissionId).ToList(),
                GroupIds = role.RoleGroups.Select(rg => rg.GroupId).ToList()
            }
        });
    }

    [HttpPost]
    [Route("api/settings/roles")]
    public async Task<IActionResult> CreateRole([FromBody] RoleCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _identityContext.Roles.AnyAsync(r => r.RoleCode == dto.RoleCode && !r.IsDeleted))
            return BadRequest(new { error = "کد نقش تکراری است" });

        var role = new Role
        {
            RoleCode = dto.RoleCode,
            RoleName = dto.RoleName,
            Description = dto.Description,
            ParentRoleId = dto.ParentRoleId,
            OrganizationId = dto.OrganizationId,
            IsActive = dto.IsActive ?? true,
            CorrespondenceTitle = dto.CorrespondenceTitle,
            DisplayTitle = dto.DisplayTitle,
            PersonnelUserId = dto.PersonnelUserId,
            DepartmentId = dto.DepartmentId
        };

        _identityContext.Roles.Add(role);
        await _identityContext.SaveChangesAsync();

        // Add permissions if provided
        if (dto.PermissionIds != null && dto.PermissionIds.Any())
        {
            foreach (var permissionId in dto.PermissionIds)
            {
                _identityContext.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permissionId });
            }
            await _identityContext.SaveChangesAsync();
        }

        // Add group assignments if provided
        if (dto.GroupIds != null && dto.GroupIds.Any())
        {
            foreach (var groupId in dto.GroupIds)
            {
                _identityContext.RoleGroups.Add(new RoleGroup { RoleId = role.Id, GroupId = groupId });
            }
            await _identityContext.SaveChangesAsync();
        }

        return Json(new { success = true, id = role.Id });
    }

    [HttpPut]
    [Route("api/settings/roles/{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleUpdateDto dto)
    {
        var role = await _identityContext.Roles.FindAsync(id);
        if (role == null || role.IsDeleted)
            return NotFound();

        if (!string.IsNullOrEmpty(dto.RoleCode) && dto.RoleCode != role.RoleCode)
        {
            if (await _identityContext.Roles.AnyAsync(r => r.RoleCode == dto.RoleCode && r.Id != id && !r.IsDeleted))
                return BadRequest(new { error = "کد نقش تکراری است" });
            role.RoleCode = dto.RoleCode;
        }

        if (!string.IsNullOrEmpty(dto.RoleName)) role.RoleName = dto.RoleName;
        if (dto.Description != null) role.Description = dto.Description;
        if (dto.ParentRoleId.HasValue) role.ParentRoleId = dto.ParentRoleId;
        if (dto.OrganizationId.HasValue) role.OrganizationId = dto.OrganizationId;
        if (dto.IsActive.HasValue) role.IsActive = dto.IsActive.Value;
        if (dto.CorrespondenceTitle != null) role.CorrespondenceTitle = dto.CorrespondenceTitle;
        if (dto.DisplayTitle != null) role.DisplayTitle = dto.DisplayTitle;
        if (dto.PersonnelUserId.HasValue) role.PersonnelUserId = dto.PersonnelUserId == 0 ? null : dto.PersonnelUserId;
        if (dto.DepartmentId.HasValue) role.DepartmentId = dto.DepartmentId == 0 ? null : dto.DepartmentId;

        role.EditDate = DateTime.UtcNow;
        await _identityContext.SaveChangesAsync();

        // Update permissions if provided
        if (dto.PermissionIds != null)
        {
            // Remove existing permissions
            var existingPermissions = await _identityContext.RolePermissions
                .Where(rp => rp.RoleId == id && !rp.IsDeleted)
                .ToListAsync();
            
            foreach (var perm in existingPermissions)
            {
                perm.IsDeleted = true;
            }

            // Add new permissions
            foreach (var permissionId in dto.PermissionIds)
            {
                _identityContext.RolePermissions.Add(new RolePermission { RoleId = id, PermissionId = permissionId });
            }
            await _identityContext.SaveChangesAsync();
        }

        // Update group assignments if provided
        if (dto.GroupIds != null)
        {
            var existingGroups = await _identityContext.RoleGroups
                .Where(rg => rg.RoleId == id && !rg.IsDeleted)
                .ToListAsync();

            foreach (var rg in existingGroups)
                rg.IsDeleted = true;

            foreach (var groupId in dto.GroupIds)
            {
                _identityContext.RoleGroups.Add(new RoleGroup { RoleId = id, GroupId = groupId });
            }
            await _identityContext.SaveChangesAsync();
        }

        return Json(new { success = true });
    }

    [HttpDelete]
    [Route("api/settings/roles/{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = await _identityContext.Roles.FindAsync(id);
        if (role == null || role.IsDeleted)
            return NotFound();

        role.IsDeleted = true;
        role.EditDate = DateTime.UtcNow;
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    /// <summary>
    /// درخت کامل سمت ها گروه‌بندی شده به سازمان (مسطح، درخت client-side ساخته می‌شه)
    /// </summary>
    [HttpGet]
    [Route("api/settings/roles/tree")]
    public async Task<IActionResult> GetRolesTree()
    {
        var organizations = await _identityContext.Organizations
            .Where(o => !o.IsDeleted && o.IsActive)
            .OrderBy(o => o.OrganizationName)
            .Select(o => new { o.Id, o.OrganizationName, o.OrganizationCode })
            .ToListAsync();

        var roles = await _identityContext.Roles
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.RoleName)
            .Select(r => new { r.Id, r.RoleName, r.RoleCode, r.ParentRoleId, r.OrganizationId, r.IsActive })
            .ToListAsync();

        return Json(new { success = true, organizations, roles });
    }

    /// <summary>
    /// فرزندان مستقیم یک سمت یا سمت های سطح اول یک سازمان
    /// </summary>
    [HttpGet]
    [Route("api/settings/roles/children")]
    public async Task<IActionResult> GetRoleChildren([FromQuery] int? parentRoleId = null, [FromQuery] int? organizationId = null)
    {
        var query = _identityContext.Roles
            .Where(r => !r.IsDeleted)
            .Include(r => r.UserRoles.Where(ur => !ur.IsDeleted))
            .AsQueryable();

        if (parentRoleId.HasValue)
            query = query.Where(r => r.ParentRoleId == parentRoleId);
        else if (organizationId.HasValue)
            query = query.Where(r => r.OrganizationId == organizationId && r.ParentRoleId == null);
        else
            query = query.Where(r => r.ParentRoleId == null);

        var roles = await query.OrderBy(r => r.RoleCode).ToListAsync();

        // تعیین اینکه آیا هر سمت زیرمجموعه دارد
        var roleIds = roles.Select(r => r.Id).ToList();
        var parentIdSet = await _identityContext.Roles
            .Where(r => !r.IsDeleted && r.ParentRoleId.HasValue && roleIds.Contains(r.ParentRoleId!.Value))
            .Select(r => r.ParentRoleId!.Value)
            .Distinct()
            .ToListAsync();

        var result = roles.Select(r => new
        {
            r.Id,
            r.RoleCode,
            r.RoleName,
            r.Description,
            r.ParentRoleId,
            r.OrganizationId,
            r.IsActive,
            UserCount = r.UserRoles.Count,
            HasChildren = parentIdSet.Contains(r.Id)
        });

        return Json(new { success = true, data = result });
    }

    /// <summary>
    /// لیست کامل گروه های کاربری (برای پیکر گروه در مدال سمت)
    /// </summary>
    [HttpGet]
    [Route("api/settings/groups")]
    public async Task<IActionResult> GetGroups()
    {
        var groups = await _identityContext.Groups
            .Where(g => !g.IsDeleted)
            .OrderBy(g => g.GroupName)
            .Select(g => new { g.Id, g.GroupCode, g.GroupName, g.ParentGroupId, g.UseInReferral, g.IsActive })
            .ToListAsync();

        return Json(new { success = true, data = groups });
    }

    // ==========================================
    //           USER / DEPARTMENT SEARCH (PICKERS)
    // ==========================================

    [HttpGet]
    [Route("api/settings/users/search")]
    public async Task<IActionResult> SearchUsers(
        [FromQuery] string? name = null,
        [FromQuery] string? username = null,
        [FromQuery] string? nationalId = null,
        [FromQuery] string? personnelNumber = null,
        [FromQuery] int? departmentId = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _identityContext.Users
            .Where(u => !u.IsDeleted)
            .Include(u => u.Department)
            .AsQueryable();

        if (!string.IsNullOrEmpty(name))
            query = query.Where(u => u.FirstName.Contains(name) || u.LastName.Contains(name));
        if (!string.IsNullOrEmpty(username))
            query = query.Where(u => u.Username.Contains(username));
        if (!string.IsNullOrEmpty(nationalId))
            query = query.Where(u => u.NationalId != null && u.NationalId.Contains(nationalId));
        if (!string.IsNullOrEmpty(personnelNumber))
            query = query.Where(u => u.PersonnelNumber != null && u.PersonnelNumber.Contains(personnelNumber));
        if (departmentId.HasValue)
            query = query.Where(u => u.DepartmentId == departmentId);

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                FullName = $"{u.FirstName} {u.LastName}",
                u.Username,
                u.NationalId,
                u.PersonnelNumber,
                u.Email,
                u.Mobile,
                DepartmentName = u.Department != null ? u.Department.DepartmentName : null,
                u.DepartmentId,
                u.IsActive
            })
            .ToListAsync();

        return Json(new { success = true, data = users, totalCount, page, pageSize });
    }

    [HttpGet]
    [Route("api/settings/departments/search")]
    public async Task<IActionResult> SearchDepartments(
        [FromQuery] string? name = null,
        [FromQuery] string? code = null,
        [FromQuery] int? organizationId = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _identityContext.Departments
            .Where(d => !d.IsDeleted)
            .Include(d => d.Organization)
            .AsQueryable();

        if (!string.IsNullOrEmpty(name))
            query = query.Where(d => d.DepartmentName.Contains(name));
        if (!string.IsNullOrEmpty(code))
            query = query.Where(d => d.DepartmentCode.Contains(code));
        if (organizationId.HasValue)
            query = query.Where(d => d.OrganizationId == organizationId);

        var totalCount = await query.CountAsync();
        var depts = await query
            .OrderBy(d => d.DepartmentName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(d => new
            {
                d.Id,
                d.DepartmentCode,
                d.DepartmentName,
                OrganizationName = d.Organization != null ? d.Organization.OrganizationName : null,
                d.OrganizationId,
                d.IsActive
            })
            .ToListAsync();

        return Json(new { success = true, data = depts, totalCount, page, pageSize });
    }

    // ==========================================
    //           USER GROUPS MANAGEMENT
    // ==========================================

    [HttpGet]
    [Route("api/settings/groups/tree")]
    public async Task<IActionResult> GetGroupsTree()
    {
        var groups = await _identityContext.Groups
            .Where(g => !g.IsDeleted)
            .Include(g => g.RoleGroups.Where(rg => !rg.IsDeleted))
            .OrderBy(g => g.GroupName)
            .Select(g => new
            {
                g.Id,
                g.GroupCode,
                g.GroupName,
                g.ParentGroupId,
                g.UseInReferral,
                g.IsActive,
                RoleCount = g.RoleGroups.Count(rg => !rg.IsDeleted)
            })
            .ToListAsync();

        return Json(new { success = true, data = groups });
    }

    [HttpGet]
    [Route("api/settings/groups/children")]
    public async Task<IActionResult> GetGroupChildren([FromQuery] int? parentGroupId = null)
    {
        var parentIdSet = new HashSet<int>(
            await _identityContext.Groups
                .Where(g => !g.IsDeleted && g.ParentGroupId.HasValue)
                .Select(g => g.ParentGroupId!.Value)
                .Distinct()
                .ToListAsync()
        );

        var groups = await _identityContext.Groups
            .Where(g => !g.IsDeleted && g.ParentGroupId == parentGroupId)
            .Include(g => g.RoleGroups.Where(rg => !rg.IsDeleted))
            .OrderBy(g => g.GroupName)
            .Select(g => new
            {
                g.Id,
                g.GroupCode,
                g.GroupName,
                g.ParentGroupId,
                g.UseInReferral,
                g.IsActive,
                RoleCount = g.RoleGroups.Count(rg => !rg.IsDeleted),
                HasChildren = parentIdSet.Contains(g.Id)
            })
            .ToListAsync();

        return Json(new { success = true, data = groups });
    }

    [HttpGet]
    [Route("api/settings/groups/{id}")]
    public async Task<IActionResult> GetGroup(int id)
    {
        var group = await _identityContext.Groups
            .Include(g => g.ParentGroup)
            .Include(g => g.RoleGroups.Where(rg => !rg.IsDeleted))
                .ThenInclude(rg => rg.Role)
            .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);

        if (group == null)
            return NotFound();

        return Json(new
        {
            success = true,
            data = new
            {
                group.Id,
                group.GroupCode,
                group.GroupName,
                group.Description,
                group.ParentGroupId,
                ParentGroupName = group.ParentGroup != null ? group.ParentGroup.GroupName : null,
                group.UseInReferral,
                group.IsActive,
                RoleIds = group.RoleGroups.Select(rg => rg.RoleId).ToList()
            }
        });
    }

    [HttpPost]
    [Route("api/settings/groups")]
    public async Task<IActionResult> CreateGroup([FromBody] GroupCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _identityContext.Groups.AnyAsync(g => g.GroupCode == dto.GroupCode && !g.IsDeleted))
            return BadRequest(new { error = "کد گروه تکراری است" });

        var group = new Group
        {
            GroupCode = dto.GroupCode,
            GroupName = dto.GroupName,
            Description = dto.Description,
            ParentGroupId = dto.ParentGroupId,
            UseInReferral = dto.UseInReferral,
            IsActive = dto.IsActive ?? true
        };

        _identityContext.Groups.Add(group);
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true, id = group.Id });
    }

    [HttpPut]
    [Route("api/settings/groups/{id}")]
    public async Task<IActionResult> UpdateGroup(int id, [FromBody] GroupUpdateDto dto)
    {
        var group = await _identityContext.Groups.FindAsync(id);
        if (group == null || group.IsDeleted)
            return NotFound();

        if (!string.IsNullOrEmpty(dto.GroupCode) && dto.GroupCode != group.GroupCode)
        {
            if (await _identityContext.Groups.AnyAsync(g => g.GroupCode == dto.GroupCode && g.Id != id && !g.IsDeleted))
                return BadRequest(new { error = "کد گروه تکراری است" });
            group.GroupCode = dto.GroupCode;
        }

        if (!string.IsNullOrEmpty(dto.GroupName)) group.GroupName = dto.GroupName;
        if (dto.Description != null) group.Description = dto.Description;
        if (dto.ParentGroupId.HasValue) group.ParentGroupId = dto.ParentGroupId == 0 ? null : dto.ParentGroupId;
        if (dto.UseInReferral.HasValue) group.UseInReferral = dto.UseInReferral.Value;
        if (dto.IsActive.HasValue) group.IsActive = dto.IsActive.Value;

        group.EditDate = DateTime.UtcNow;
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpDelete]
    [Route("api/settings/groups/{id}")]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        var group = await _identityContext.Groups.FindAsync(id);
        if (group == null || group.IsDeleted)
            return NotFound();

        group.IsDeleted = true;
        group.EditDate = DateTime.UtcNow;
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpGet]
    [Route("api/settings/groups/{id}/roles")]
    public async Task<IActionResult> GetGroupRoles(int id)
    {
        var roleGroups = await _identityContext.RoleGroups
            .Where(rg => rg.GroupId == id && !rg.IsDeleted)
            .Include(rg => rg.Role)
            .Select(rg => new
            {
                rg.Id,
                rg.RoleId,
                RoleName = rg.Role.RoleName,
                RoleCode = rg.Role.RoleCode
            })
            .ToListAsync();

        return Json(new { success = true, data = roleGroups });
    }

    [HttpPost]
    [Route("api/settings/groups/{groupId}/roles/{roleId}")]
    public async Task<IActionResult> AddRoleToGroup(int groupId, int roleId)
    {
        if (await _identityContext.RoleGroups.AnyAsync(rg => rg.GroupId == groupId && rg.RoleId == roleId && !rg.IsDeleted))
            return BadRequest(new { error = "این نقش قبلاً به گروه اضافه شده است" });

        _identityContext.RoleGroups.Add(new RoleGroup { GroupId = groupId, RoleId = roleId });
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpDelete]
    [Route("api/settings/groups/{groupId}/roles/{roleId}")]
    public async Task<IActionResult> RemoveRoleFromGroup(int groupId, int roleId)
    {
        var rg = await _identityContext.RoleGroups
            .FirstOrDefaultAsync(r => r.GroupId == groupId && r.RoleId == roleId && !r.IsDeleted);
        if (rg == null) return NotFound();

        rg.IsDeleted = true;
        rg.EditDate = DateTime.UtcNow;
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpGet]
    [Route("api/settings/groups/{id}/permissions")]
    public async Task<IActionResult> GetGroupPermissions(int id)
    {
        var permissions = await _identityContext.GroupPermissions
            .Where(gp => gp.GroupId == id && !gp.IsDeleted)
            .Include(gp => gp.Permission)
            .Select(gp => new
            {
                gp.PermissionId,
                PermissionCode = gp.Permission.PermissionCode,
                PermissionName = gp.Permission.PermissionName,
                Category = gp.Permission.Category
            })
            .ToListAsync();

        return Json(new { success = true, data = permissions });
    }

    // ==========================================
    //           PERMISSIONS ASSIGNMENTS
    // ==========================================

    [HttpGet]
    [Route("api/settings/permissions/by-category")]
    public async Task<IActionResult> GetPermissionsByCategory()
    {
        var permissions = await _identityContext.Permissions
            .Where(p => !p.IsDeleted)
            .Include(p => p.PermissionGroup)
            .OrderBy(p => p.PermissionGroup != null ? p.PermissionGroup.DisplayOrder : 999)
            .ThenBy(p => p.DisplayOrder)
            .ThenBy(p => p.PermissionName)
            .Select(p => new
            {
                p.Id,
                p.PermissionCode,
                p.PermissionName,
                p.Description,
                p.Category,
                p.PermissionGroupId,
                PermissionGroupName = p.PermissionGroup != null ? p.PermissionGroup.GroupName : null,
                p.IsActive
            })
            .ToListAsync();

        return Json(new { success = true, data = permissions });
    }

    [HttpGet]
    [Route("api/settings/permissions/{id}/assignments")]
    public async Task<IActionResult> GetPermissionAssignments(int id)
    {
        var roles = await _identityContext.RolePermissions
            .Where(rp => rp.PermissionId == id && !rp.IsDeleted)
            .Include(rp => rp.Role)
            .Select(rp => new
            {
                rp.Id,
                rp.RoleId,
                RoleName = rp.Role.RoleName,
                RoleCode = rp.Role.RoleCode
            })
            .ToListAsync();

        var groups = await _identityContext.GroupPermissions
            .Where(gp => gp.PermissionId == id && !gp.IsDeleted)
            .Include(gp => gp.Group)
            .Select(gp => new
            {
                gp.Id,
                gp.GroupId,
                GroupName = gp.Group.GroupName,
                GroupCode = gp.Group.GroupCode
            })
            .ToListAsync();

        return Json(new { success = true, roles, groups });
    }

    [HttpPost]
    [Route("api/settings/permissions/{permissionId}/assign-role/{roleId}")]
    public async Task<IActionResult> AssignPermissionToRole(int permissionId, int roleId)
    {
        if (await _identityContext.RolePermissions.AnyAsync(rp => rp.PermissionId == permissionId && rp.RoleId == roleId && !rp.IsDeleted))
            return BadRequest(new { error = "این نقش قبلاً این مجوز رو داره" });

        _identityContext.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permissionId });
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpDelete]
    [Route("api/settings/permissions/{permissionId}/role/{roleId}")]
    public async Task<IActionResult> RemovePermissionFromRole(int permissionId, int roleId)
    {
        var rp = await _identityContext.RolePermissions
            .FirstOrDefaultAsync(r => r.PermissionId == permissionId && r.RoleId == roleId && !r.IsDeleted);
        if (rp == null) return NotFound();

        rp.IsDeleted = true;
        rp.EditDate = DateTime.UtcNow;
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpPost]
    [Route("api/settings/permissions/{permissionId}/assign-group/{groupId}")]
    public async Task<IActionResult> AssignPermissionToGroup(int permissionId, int groupId)
    {
        if (await _identityContext.GroupPermissions.AnyAsync(gp => gp.PermissionId == permissionId && gp.GroupId == groupId && !gp.IsDeleted))
            return BadRequest(new { error = "این گروه قبلاً این مجوز رو داره" });

        _identityContext.GroupPermissions.Add(new GroupPermission { GroupId = groupId, PermissionId = permissionId });
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpDelete]
    [Route("api/settings/permissions/{permissionId}/group/{groupId}")]
    public async Task<IActionResult> RemovePermissionFromGroup(int permissionId, int groupId)
    {
        var gp = await _identityContext.GroupPermissions
            .FirstOrDefaultAsync(g => g.PermissionId == permissionId && g.GroupId == groupId && !g.IsDeleted);
        if (gp == null) return NotFound();

        gp.IsDeleted = true;
        gp.EditDate = DateTime.UtcNow;
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    // ==========================================
    //           ORGANIZATIONS MANAGEMENT
    // ==========================================

    [HttpGet]
    [Route("settings/organizations")]
    public IActionResult Organizations()
    {
        return View();
    }

    [HttpGet]
    [Route("api/settings/organizations")]
    public async Task<IActionResult> GetOrganizations([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var query = _identityContext.Organizations
            .Where(o => !o.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(o => 
                o.OrganizationCode.Contains(search) ||
                o.OrganizationName.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var organizations = await query
            .OrderBy(o => o.OrganizationCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new
            {
                o.Id,
                o.OrganizationCode,
                o.OrganizationName,
                o.IsParentOrganization,
                o.IsSubsidiary,
                o.Description,
                o.IsActive,
                o.CreationDate
            })
            .ToListAsync();

        return Json(new { success = true, data = organizations, totalCount, page, pageSize });
    }

    [HttpGet]
    [Route("api/settings/organizations/{id}")]
    public async Task<IActionResult> GetOrganization(int id)
    {
        var org = await _identityContext.Organizations
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

        if (org == null)
            return NotFound();

        return Json(new { success = true, data = org });
    }

    [HttpPost]
    [Route("api/settings/organizations")]
    public async Task<IActionResult> CreateOrganization([FromBody] OrganizationCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _identityContext.Organizations.AnyAsync(o => o.OrganizationCode == dto.OrganizationCode && !o.IsDeleted))
            return BadRequest(new { error = "کد سازمان تکراری است" });

        var org = new Organization
        {
            OrganizationCode = dto.OrganizationCode,
            OrganizationName = dto.OrganizationName,
            IsParentOrganization = dto.IsParentOrganization,
            IsSubsidiary = dto.IsSubsidiary,
            Description = dto.Description,
            IsActive = dto.IsActive ?? true
        };

        _identityContext.Organizations.Add(org);
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true, id = org.Id });
    }

    [HttpPut]
    [Route("api/settings/organizations/{id}")]
    public async Task<IActionResult> UpdateOrganization(int id, [FromBody] OrganizationUpdateDto dto)
    {
        var org = await _identityContext.Organizations.FindAsync(id);
        if (org == null || org.IsDeleted)
            return NotFound();

        if (!string.IsNullOrEmpty(dto.OrganizationCode) && dto.OrganizationCode != org.OrganizationCode)
        {
            if (await _identityContext.Organizations.AnyAsync(o => o.OrganizationCode == dto.OrganizationCode && o.Id != id && !o.IsDeleted))
                return BadRequest(new { error = "کد سازمان تکراری است" });
            org.OrganizationCode = dto.OrganizationCode;
        }

        if (!string.IsNullOrEmpty(dto.OrganizationName)) org.OrganizationName = dto.OrganizationName;
        if (dto.IsParentOrganization.HasValue) org.IsParentOrganization = dto.IsParentOrganization.Value;
        if (dto.IsSubsidiary.HasValue) org.IsSubsidiary = dto.IsSubsidiary.Value;
        if (dto.Description != null) org.Description = dto.Description;
        if (dto.IsActive.HasValue) org.IsActive = dto.IsActive.Value;

        org.EditDate = DateTime.UtcNow;
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpDelete]
    [Route("api/settings/organizations/{id}")]
    public async Task<IActionResult> DeleteOrganization(int id)
    {
        var org = await _identityContext.Organizations.FindAsync(id);
        if (org == null || org.IsDeleted)
            return NotFound();

        org.IsDeleted = true;
        org.EditDate = DateTime.UtcNow;
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    // ==========================================
    //           DEPARTMENTS MANAGEMENT
    // ==========================================

    [HttpGet]
    [Route("settings/departments")]
    public IActionResult Departments()
    {
        return View();
    }

    [HttpGet]
    [Route("api/settings/departments")]
    public async Task<IActionResult> GetDepartments([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var query = _identityContext.Departments
            .Where(d => !d.IsDeleted)
            .Include(d => d.Organization)
            .Include(d => d.ParentDepartment)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(d =>
                d.DepartmentCode.Contains(search) ||
                d.DepartmentName.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var departments = await query
            .OrderBy(d => d.DepartmentCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new
            {
                d.Id,
                d.DepartmentCode,
                d.DepartmentName,
                d.Description,
                OrganizationName = d.Organization != null ? d.Organization.OrganizationName : null,
                d.OrganizationId,
                d.ParentDepartmentId,
                ParentDepartmentName = d.ParentDepartment != null ? d.ParentDepartment.DepartmentName : null,
                d.IsActive,
                d.CreationDate
            })
            .ToListAsync();

        return Json(new { success = true, data = departments, totalCount, page, pageSize });
    }

    [HttpGet]
    [Route("api/settings/departments/{id}")]
    public async Task<IActionResult> GetDepartment(int id)
    {
        var dept = await _identityContext.Departments
            .Include(d => d.Organization)
            .Include(d => d.ParentDepartment)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        if (dept == null)
            return NotFound();

        return Json(new
        {
            success = true,
            data = new
            {
                dept.Id,
                dept.DepartmentCode,
                dept.DepartmentName,
                dept.Description,
                dept.OrganizationId,
                OrganizationName = dept.Organization?.OrganizationName,
                dept.ParentDepartmentId,
                ParentDepartmentName = dept.ParentDepartment?.DepartmentName,
                dept.IsActive,
                dept.CreationDate
            }
        });
    }

    [HttpPost]
    [Route("api/settings/departments")]
    public async Task<IActionResult> CreateDepartment([FromBody] DepartmentCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _identityContext.Departments.AnyAsync(d => d.DepartmentCode == dto.DepartmentCode && !d.IsDeleted))
            return BadRequest(new { error = "کد واحد تکراری است" });

        var dept = new Department
        {
            DepartmentCode = dto.DepartmentCode,
            DepartmentName = dto.DepartmentName,
            Description = dto.Description,
            OrganizationId = dto.OrganizationId,
            ParentDepartmentId = dto.ParentDepartmentId,
            IsActive = dto.IsActive ?? true
        };

        _identityContext.Departments.Add(dept);
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true, id = dept.Id });
    }

    [HttpPut]
    [Route("api/settings/departments/{id}")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] DepartmentUpdateDto dto)
    {
        var dept = await _identityContext.Departments.FindAsync(id);
        if (dept == null || dept.IsDeleted)
            return NotFound();

        if (!string.IsNullOrEmpty(dto.DepartmentCode) && dto.DepartmentCode != dept.DepartmentCode)
        {
            if (await _identityContext.Departments.AnyAsync(d => d.DepartmentCode == dto.DepartmentCode && d.Id != id && !d.IsDeleted))
                return BadRequest(new { error = "کد واحد تکراری است" });
            dept.DepartmentCode = dto.DepartmentCode;
        }

        if (!string.IsNullOrEmpty(dto.DepartmentName)) dept.DepartmentName = dto.DepartmentName;
        if (dto.Description != null) dept.Description = dto.Description;
        if (dto.OrganizationId.HasValue) dept.OrganizationId = dto.OrganizationId;
        if (dto.ParentDepartmentId.HasValue) dept.ParentDepartmentId = dto.ParentDepartmentId == 0 ? null : dto.ParentDepartmentId;
        if (dto.IsActive.HasValue) dept.IsActive = dto.IsActive.Value;

        dept.EditDate = DateTime.UtcNow;
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpDelete]
    [Route("api/settings/departments/{id}")]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var dept = await _identityContext.Departments.FindAsync(id);
        if (dept == null || dept.IsDeleted)
            return NotFound();

        dept.IsDeleted = true;
        dept.EditDate = DateTime.UtcNow;
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    // ==========================================
    //      DEPARTMENT TREE & ROLE ASSIGNMENT
    // ==========================================

    /// <summary>
    /// درخت کامل واحدهای سازمانی گروه‌بندی شده به سازمان
    /// </summary>
    [HttpGet]
    [Route("api/settings/departments/tree")]
    public async Task<IActionResult> GetDepartmentsTree()
    {
        var organizations = await _identityContext.Organizations
            .Where(o => !o.IsDeleted && o.IsActive)
            .OrderBy(o => o.OrganizationName)
            .Select(o => new { o.Id, o.OrganizationName, o.OrganizationCode })
            .ToListAsync();

        var departments = await _identityContext.Departments
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.DepartmentName)
            .Select(d => new { d.Id, d.DepartmentName, d.DepartmentCode, d.ParentDepartmentId, d.OrganizationId, d.IsActive })
            .ToListAsync();

        return Json(new { success = true, organizations, departments });
    }

    /// <summary>
    /// فرزندان مستقیم یک واحد یا واحدهای سطح اول یک سازمان
    /// </summary>
    [HttpGet]
    [Route("api/settings/departments/children")]
    public async Task<IActionResult> GetDepartmentChildren(
        [FromQuery] int? parentDepartmentId = null,
        [FromQuery] int? organizationId = null)
    {
        var query = _identityContext.Departments
            .Where(d => !d.IsDeleted)
            .AsQueryable();

        if (parentDepartmentId.HasValue)
            query = query.Where(d => d.ParentDepartmentId == parentDepartmentId);
        else if (organizationId.HasValue)
            query = query.Where(d => d.OrganizationId == organizationId && d.ParentDepartmentId == null);
        else
            query = query.Where(d => d.ParentDepartmentId == null);

        var departments = await query
            .Include(d => d.Organization)
            .OrderBy(d => d.DepartmentCode)
            .ToListAsync();

        var deptIds = departments.Select(d => d.Id).ToList();
        var parentIdSet = await _identityContext.Departments
            .Where(d => !d.IsDeleted && d.ParentDepartmentId.HasValue && deptIds.Contains(d.ParentDepartmentId!.Value))
            .Select(d => d.ParentDepartmentId!.Value)
            .Distinct()
            .ToListAsync();

        // Count roles per department
        var roleCounts = await _identityContext.Roles
            .Where(r => !r.IsDeleted && r.DepartmentId.HasValue && deptIds.Contains(r.DepartmentId!.Value))
            .GroupBy(r => r.DepartmentId!.Value)
            .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
            .ToListAsync();
        var roleCountDict = roleCounts.ToDictionary(x => x.DepartmentId, x => x.Count);

        var result = departments.Select(d => new
        {
            d.Id,
            d.DepartmentCode,
            d.DepartmentName,
            d.Description,
            d.ParentDepartmentId,
            d.OrganizationId,
            OrganizationName = d.Organization?.OrganizationName,
            d.IsActive,
            RoleCount = roleCountDict.GetValueOrDefault(d.Id, 0),
            HasChildren = parentIdSet.Contains(d.Id)
        });

        return Json(new { success = true, data = result });
    }

    /// <summary>
    /// لیست سمت‌های یک واحد سازمانی
    /// </summary>
    [HttpGet]
    [Route("api/settings/departments/{id}/roles")]
    public async Task<IActionResult> GetDepartmentRoles(int id)
    {
        var roles = await _identityContext.Roles
            .Where(r => !r.IsDeleted && r.DepartmentId == id)
            .Include(r => r.PersonnelUser)
            .OrderBy(r => r.RoleCode)
            .Select(r => new
            {
                r.Id,
                r.RoleCode,
                r.RoleName,
                r.DisplayTitle,
                PersonnelName = r.PersonnelUser != null ? r.PersonnelUser.FirstName + " " + r.PersonnelUser.LastName : null,
                r.IsActive
            })
            .ToListAsync();

        return Json(new { success = true, data = roles });
    }

    /// <summary>
    /// انتساب سمت به واحد سازمانی
    /// </summary>
    [HttpPost]
    [Route("api/settings/departments/{departmentId}/roles/{roleId}")]
    public async Task<IActionResult> AssignRoleToDepartment(int departmentId, int roleId)
    {
        var role = await _identityContext.Roles.FindAsync(roleId);
        if (role == null || role.IsDeleted) return NotFound();

        role.DepartmentId = departmentId;
        role.EditDate = DateTime.UtcNow;
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    /// <summary>
    /// حذف انتساب سمت از واحد سازمانی
    /// </summary>
    [HttpDelete]
    [Route("api/settings/departments/{departmentId}/roles/{roleId}")]
    public async Task<IActionResult> RemoveRoleFromDepartment(int departmentId, int roleId)
    {
        var role = await _identityContext.Roles.FindAsync(roleId);
        if (role == null || role.IsDeleted || role.DepartmentId != departmentId) return NotFound();

        role.DepartmentId = null;
        role.EditDate = DateTime.UtcNow;
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    /// <summary>
    /// جستجوی سمت‌ها (برای پیکر در واحد سازمانی)
    /// </summary>
    [HttpGet]
    [Route("api/settings/roles/search")]
    public async Task<IActionResult> SearchRoles(
        [FromQuery] string? name = null,
        [FromQuery] string? code = null,
        [FromQuery] int? organizationId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _identityContext.Roles
            .Where(r => !r.IsDeleted)
            .Include(r => r.Organization)
            .Include(r => r.Department)
            .AsQueryable();

        if (!string.IsNullOrEmpty(name))
            query = query.Where(r => r.RoleName.Contains(name));
        if (!string.IsNullOrEmpty(code))
            query = query.Where(r => r.RoleCode.Contains(code));
        if (organizationId.HasValue)
            query = query.Where(r => r.OrganizationId == organizationId);

        var totalCount = await query.CountAsync();
        var roles = await query
            .OrderBy(r => r.RoleName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.RoleCode,
                r.RoleName,
                r.DisplayTitle,
                OrganizationName = r.Organization != null ? r.Organization.OrganizationName : null,
                DepartmentName = r.Department != null ? r.Department.DepartmentName : null,
                r.DepartmentId,
                r.IsActive
            })
            .ToListAsync();

        return Json(new { success = true, data = roles, totalCount, page, pageSize });
    }

    // ==========================================
    //           HELPER ENDPOINTS
    // ==========================================

    [HttpGet]
    [Route("api/settings/organizations/list")]
    public async Task<IActionResult> GetOrganizationsList()
    {
        var orgs = await _identityContext.Organizations
            .Where(o => !o.IsDeleted && o.IsActive)
            .OrderBy(o => o.OrganizationName)
            .Select(o => new { o.Id, o.OrganizationName, o.OrganizationCode })
            .ToListAsync();

        return Json(new { success = true, data = orgs });
    }

    [HttpGet]
    [Route("api/settings/departments/list")]
    public async Task<IActionResult> GetDepartmentsList([FromQuery] int? organizationId = null)
    {
        var query = _identityContext.Departments
            .Where(d => !d.IsDeleted && d.IsActive)
            .AsQueryable();

        if (organizationId.HasValue)
            query = query.Where(d => d.OrganizationId == organizationId);

        var depts = await query
            .OrderBy(d => d.DepartmentName)
            .Select(d => new { d.Id, d.DepartmentName, d.DepartmentCode, d.OrganizationId })
            .ToListAsync();

        return Json(new { success = true, data = depts });
    }

    [HttpGet]
    [Route("api/settings/roles/list")]
    public async Task<IActionResult> GetRolesList()
    {
        var roles = await _identityContext.Roles
            .Where(r => !r.IsDeleted && r.IsActive)
            .OrderBy(r => r.RoleName)
            .Select(r => new { r.Id, r.RoleName, r.RoleCode })
            .ToListAsync();

        return Json(new { success = true, data = roles });
    }

    [HttpGet]
    [Route("api/settings/permissions/list")]
    public async Task<IActionResult> GetPermissionsList()
    {
        var permissions = await _identityContext.Permissions
            .Where(p => !p.IsDeleted && p.IsActive)
            .OrderBy(p => p.PermissionName)
            .Select(p => new { p.Id, p.PermissionName, p.PermissionCode, p.Category })
            .ToListAsync();

        return Json(new { success = true, data = permissions });
    }

    // ==========================================
    //           PERMISSIONS MANAGEMENT
    // ==========================================

    [HttpGet]
    [Route("settings/permissions")]
    public IActionResult Permissions()
    {
        return View();
    }

    [HttpGet]
    [Route("api/settings/permissions")]
    public async Task<IActionResult> GetPermissions([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? category = null)
    {
        var query = _identityContext.Permissions
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => 
                p.PermissionCode.Contains(search) ||
                p.PermissionName.Contains(search) ||
                (p.Description != null && p.Description.Contains(search)));
        }

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category == category);
        }

        var totalCount = await query.CountAsync();
        var permissions = await query
            .OrderBy(p => p.Category)
            .ThenBy(p => p.PermissionName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.PermissionCode,
                p.PermissionName,
                p.Description,
                p.Category,
                p.IsActive,
                p.CreationDate
            })
            .ToListAsync();

        return Json(new { success = true, data = permissions, totalCount, page, pageSize });
    }

    [HttpGet]
    [Route("api/settings/permissions/{id}")]
    public async Task<IActionResult> GetPermission(int id)
    {
        var permission = await _identityContext.Permissions
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (permission == null)
            return NotFound();

        return Json(new { success = true, data = permission });
    }

    [HttpPost]
    [Route("api/settings/permissions")]
    public async Task<IActionResult> CreatePermission([FromBody] PermissionCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _identityContext.Permissions.AnyAsync(p => p.PermissionCode == dto.PermissionCode && !p.IsDeleted))
            return BadRequest(new { error = "کد مجوز تکراری است" });

        var permission = new Permission
        {
            PermissionCode = dto.PermissionCode,
            PermissionName = dto.PermissionName,
            Description = dto.Description,
            Category = dto.Category,
            IsActive = dto.IsActive ?? true
        };

        _identityContext.Permissions.Add(permission);
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true, id = permission.Id });
    }

    [HttpPut]
    [Route("api/settings/permissions/{id}")]
    public async Task<IActionResult> UpdatePermission(int id, [FromBody] PermissionUpdateDto dto)
    {
        var permission = await _identityContext.Permissions.FindAsync(id);
        if (permission == null || permission.IsDeleted)
            return NotFound();

        if (!string.IsNullOrEmpty(dto.PermissionCode) && dto.PermissionCode != permission.PermissionCode)
        {
            if (await _identityContext.Permissions.AnyAsync(p => p.PermissionCode == dto.PermissionCode && p.Id != id && !p.IsDeleted))
                return BadRequest(new { error = "کد مجوز تکراری است" });
            permission.PermissionCode = dto.PermissionCode;
        }

        if (!string.IsNullOrEmpty(dto.PermissionName)) permission.PermissionName = dto.PermissionName;
        if (dto.Description != null) permission.Description = dto.Description;
        if (dto.Category != null) permission.Category = dto.Category;
        if (dto.IsActive.HasValue) permission.IsActive = dto.IsActive.Value;

        permission.EditDate = DateTime.UtcNow;
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpDelete]
    [Route("api/settings/permissions/{id}")]
    public async Task<IActionResult> DeletePermission(int id)
    {
        var permission = await _identityContext.Permissions.FindAsync(id);
        if (permission == null || permission.IsDeleted)
            return NotFound();

        permission.IsDeleted = true;
        permission.EditDate = DateTime.UtcNow;
        await _identityContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpGet]
    [Route("api/settings/permissions/categories")]
    public async Task<IActionResult> GetPermissionCategories()
    {
        var categories = await _identityContext.Permissions
            .Where(p => !p.IsDeleted && !string.IsNullOrEmpty(p.Category))
            .Select(p => p.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Json(new { success = true, data = categories });
    }

    /// <summary>
    /// لیست فرم‌ها برای درخت مجوزهای فرمساز (از جدول Forms)
    /// </summary>
    [HttpGet]
    [Route("api/settings/permissions/formbuilder-tree")]
    public async Task<IActionResult> GetFormbuilderFormsTree()
    {
        // Load all forms from Forms table
        var forms = await _context.Forms
            .Where(f => !f.IsDeleted)
            .OrderBy(f => f.NameFa)
            .Select(f => new
            {
                f.Id,
                f.FormCode,
                f.NameFa,
                f.NameEn,
                CategoryName = f.Category != null ? f.Category.NameFa : null
            })
            .ToListAsync();

        // Load permission groups for formbuilder (FRMB_ prefix) to match forms
        var permGroups = await _identityContext.PermissionGroups
            .Where(pg => !pg.IsDeleted && pg.GroupCode.StartsWith("FRMB_"))
            .Select(pg => new { pg.Id, pg.GroupCode })
            .ToListAsync();

        // Load permission counts per group
        var permCounts = await _identityContext.Permissions
            .Where(p => !p.IsDeleted && p.Category == "formbuilder" && p.PermissionGroupId.HasValue)
            .GroupBy(p => p.PermissionGroupId)
            .Select(g => new { GroupId = g.Key, Count = g.Count() })
            .ToListAsync();

        var result = forms.Select(f => {
            var pg = permGroups.FirstOrDefault(g => g.GroupCode == $"FRMB_{f.FormCode}");
            var count = pg != null ? permCounts.FirstOrDefault(c => c.GroupId == pg.Id)?.Count ?? 0 : 0;
            return new
            {
                f.Id,
                f.FormCode,
                f.NameFa,
                f.NameEn,
                f.CategoryName,
                PermissionGroupId = pg?.Id,
                PermissionCount = count
            };
        }).ToList();

        return Json(new { success = true, data = result });
    }

    /// <summary>
    /// مجوزهای یک فرم خاص بر اساس FormCode
    /// </summary>
    [HttpGet]
    [Route("api/settings/permissions/by-form/{formCode}")]
    public async Task<IActionResult> GetPermissionsByForm(string formCode)
    {
        var groupCode = $"FRMB_{formCode}";
        var permGroup = await _identityContext.PermissionGroups
            .FirstOrDefaultAsync(pg => pg.GroupCode == groupCode && !pg.IsDeleted);

        if (permGroup == null)
            return Json(new { success = true, data = new List<object>() });

        var permissions = await _identityContext.Permissions
            .Where(p => !p.IsDeleted && p.PermissionGroupId == permGroup.Id && p.Category == "formbuilder")
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new
            {
                p.Id,
                p.PermissionCode,
                p.PermissionName,
                p.Description,
                p.IsActive
            })
            .ToListAsync();

        return Json(new { success = true, data = permissions });
    }

    /// <summary>
    /// درخت سمت‌ها برای پیکر (سازمان → واحد → سمت)
    /// </summary>
    [HttpGet]
    [Route("api/settings/roles/tree-picker")]
    public async Task<IActionResult> GetRolesTreePicker([FromQuery] string? search = null)
    {
        var query = _identityContext.Roles
            .Where(r => !r.IsDeleted)
            .Include(r => r.Organization)
            .Include(r => r.Department)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(r =>
                r.RoleCode.Contains(search) ||
                r.RoleName.Contains(search) ||
                (r.CorrespondenceTitle != null && r.CorrespondenceTitle.Contains(search)) ||
                (r.DisplayTitle != null && r.DisplayTitle.Contains(search)));
        }

        var roles = await query
            .OrderBy(r => r.RoleName)
            .Select(r => new
            {
                r.Id,
                r.RoleCode,
                r.RoleName,
                r.CorrespondenceTitle,
                r.DisplayTitle,
                r.ParentRoleId,
                r.OrganizationId,
                OrganizationName = r.Organization != null ? r.Organization.OrganizationName : null,
                r.DepartmentId,
                DepartmentName = r.Department != null ? r.Department.DepartmentName : null,
                r.IsActive
            })
            .ToListAsync();

        return Json(new { success = true, data = roles });
    }

    /// <summary>
    /// درخت گروه‌ها برای پیکر (با تعداد سمت‌ها)
    /// </summary>
    [HttpGet]
    [Route("api/settings/groups/tree-picker")]
    public async Task<IActionResult> GetGroupsTreePicker([FromQuery] string? search = null)
    {
        var query = _identityContext.Groups
            .Where(g => !g.IsDeleted)
            .Include(g => g.RoleGroups.Where(rg => !rg.IsDeleted))
                .ThenInclude(rg => rg.Role)
            .AsQueryable();

        var groups = await query
            .OrderBy(g => g.GroupName)
            .Select(g => new
            {
                g.Id,
                g.GroupCode,
                g.GroupName,
                g.ParentGroupId,
                g.UseInReferral,
                g.IsActive,
                Roles = g.RoleGroups.Where(rg => !rg.IsDeleted).Select(rg => new
                {
                    rg.RoleId,
                    RoleName = rg.Role != null ? rg.Role.RoleName : null
                }).ToList()
            })
            .ToListAsync();

        // Filter after loading if search is provided (to search by role names in group)
        if (!string.IsNullOrEmpty(search))
        {
            groups = groups.Where(g =>
                g.GroupName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                g.GroupCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                g.Roles.Any(r => r.RoleName != null && r.RoleName.Contains(search, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        return Json(new { success = true, data = groups });
    }

    /// <summary>
    /// Get hierarchical structure: Organizations -> Departments -> Users
    /// </summary>
    [HttpGet]
    [Route("api/settings/organizational-tree")]
    public async Task<IActionResult> GetOrganizationalTree()
    {
        var organizations = await _identityContext.Organizations
            .Where(o => !o.IsDeleted && o.IsActive)
            .Include(o => o.Departments.Where(d => !d.IsDeleted && d.IsActive))
                .ThenInclude(d => d.Users.Where(u => !u.IsDeleted))
                    .ThenInclude(u => u.UserRoles.Where(ur => !ur.IsDeleted))
                        .ThenInclude(ur => ur.Role)
            .OrderBy(o => o.OrganizationName)
            .ToListAsync();

        var tree = organizations.Select(org => new
        {
            id = $"org_{org.Id}",
            type = "organization",
            text = org.OrganizationName,
            code = org.OrganizationCode,
            icon = "bi-building",
            children = org.Departments.Select(dept => new
            {
                id = $"dept_{dept.Id}",
                type = "department",
                text = dept.DepartmentName,
                code = dept.DepartmentCode,
                icon = "bi-diagram-3",
                children = dept.Users.Select(user => new
                {
                    id = $"user_{user.Id}",
                    type = "user",
                    text = $"{user.FirstName} {user.LastName}",
                    username = user.Username,
                    email = user.Email,
                    mobile = user.Mobile,
                    isActive = user.IsActive,
                    icon = "bi-person",
                    roles = user.UserRoles.Select(ur => new
                    {
                        ur.RoleId,
                        RoleName = ur.Role != null ? ur.Role.RoleName : null,
                        RoleCode = ur.Role != null ? ur.Role.RoleCode : null
                    }).ToList()
                }).OrderBy(u => u.text).ToList()
            }).OrderBy(d => d.text).ToList()
        }).ToList();

        return Json(new { success = true, data = tree });
    }

    // ==========================================
    //           POSITIONS MANAGEMENT
    // ==========================================

    [HttpGet]
    [Route("api/settings/positions/tree")]
    public async Task<IActionResult> GetPositionsTree()
    {
        var organizations = await _identityContext.Organizations
            .Where(o => !o.IsDeleted)
            .OrderBy(o => o.OrganizationName)
            .Select(o => new { o.Id, o.OrganizationName })
            .ToListAsync();

        var positions = await _identityContext.Positions
            .Where(p => !p.IsDeleted)
            .Include(p => p.Organization)
            .Include(p => p.Department)
            .Select(p => new
            {
                p.Id,
                p.PositionCode,
                p.PositionName,
                p.ParentPositionId,
                p.OrganizationId,
                OrganizationName = p.Organization != null ? p.Organization.OrganizationName : null,
                p.DepartmentId,
                DepartmentName = p.Department != null ? p.Department.DepartmentName : null,
                p.IsActive
            })
            .ToListAsync();

        return Json(new { success = true, data = new { organizations, positions } });
    }

    [HttpGet]
    [Route("api/settings/positions/children")]
    public async Task<IActionResult> GetPositionChildren([FromQuery] int? parentId = null, [FromQuery] int? orgId = null)
    {
        var query = _identityContext.Positions
            .Where(p => !p.IsDeleted)
            .Include(p => p.Organization)
            .Include(p => p.Department)
            .Include(p => p.UserPositions.Where(up => !up.IsDeleted))
            .AsQueryable();

        if (parentId.HasValue)
            query = query.Where(p => p.ParentPositionId == parentId.Value);
        else if (orgId.HasValue)
            query = query.Where(p => p.OrganizationId == orgId.Value && p.ParentPositionId == null);
        else
            query = query.Where(p => p.ParentPositionId == null);

        var list = await query
            .OrderBy(p => p.PositionName)
            .Select(p => new
            {
                p.Id,
                p.PositionCode,
                p.PositionName,
                p.Description,
                p.ParentPositionId,
                p.OrganizationId,
                OrganizationName = p.Organization != null ? p.Organization.OrganizationName : null,
                p.DepartmentId,
                DepartmentName = p.Department != null ? p.Department.DepartmentName : null,
                p.IsActive,
                UserCount = p.UserPositions.Count(up => !up.IsDeleted)
            })
            .ToListAsync();

        return Json(new { success = true, data = list });
    }

    [HttpGet]
    [Route("api/settings/positions/{id}")]
    public async Task<IActionResult> GetPosition(int id)
    {
        var position = await _identityContext.Positions
            .Include(p => p.Organization)
            .Include(p => p.Department)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (position == null)
            return NotFound();

        var roles = await _identityContext.RolePositions
            .Where(rp => rp.PositionId == id && !rp.IsDeleted)
            .Include(rp => rp.Role)
            .Select(rp => new
            {
                rp.RoleId,
                RoleName = rp.Role != null ? rp.Role.RoleName : string.Empty
            })
            .ToListAsync();

        var users = await _identityContext.UserPositions
            .Where(up => up.PositionId == id && !up.IsDeleted)
            .Include(up => up.User)
            .Select(up => new
            {
                up.UserId,
                FullName = up.User != null ? (up.User.FirstName + " " + up.User.LastName) : string.Empty
            })
            .ToListAsync();

        return Json(new
        {
            success = true,
            data = new
            {
                position.Id,
                position.PositionCode,
                position.PositionName,
                position.Description,
                position.ParentPositionId,
                position.OrganizationId,
                OrganizationName = position.Organization != null ? position.Organization.OrganizationName : null,
                position.DepartmentId,
                DepartmentName = position.Department != null ? position.Department.DepartmentName : null,
                position.IsActive,
                RoleIds = roles.Select(r => r.RoleId).ToList(),
                UserIds = users.Select(u => u.UserId).ToList(),
                Roles = roles,
                Users = users
            }
        });
    }

    [HttpPost]
    [Route("api/settings/positions")]
    public async Task<IActionResult> CreatePosition([FromBody] PositionCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _identityContext.Positions.AnyAsync(p => p.PositionCode == dto.PositionCode && !p.IsDeleted))
            return BadRequest(new { error = "کد پست تکراری است" });

        var position = new Position
        {
            PositionCode = dto.PositionCode,
            PositionName = dto.PositionName,
            Description = dto.Description,
            ParentPositionId = dto.ParentPositionId,
            OrganizationId = dto.OrganizationId,
            DepartmentId = dto.DepartmentId,
            IsActive = dto.IsActive ?? true
        };

        _identityContext.Positions.Add(position);
        await _identityContext.SaveChangesAsync();

        if (dto.RoleIds != null && dto.RoleIds.Any())
        {
            foreach (var roleId in dto.RoleIds.Distinct())
            {
                _identityContext.RolePositions.Add(new RolePosition { RoleId = roleId, PositionId = position.Id });
            }
        }

        if (dto.UserIds != null && dto.UserIds.Any())
        {
            foreach (var userId in dto.UserIds.Distinct())
            {
                _identityContext.UserPositions.Add(new UserPosition { UserId = userId, PositionId = position.Id, IsPrimary = false });
            }
        }

        await _identityContext.SaveChangesAsync();
        return Json(new { success = true, id = position.Id });
    }

    [HttpPut]
    [Route("api/settings/positions/{id}")]
    public async Task<IActionResult> UpdatePosition(int id, [FromBody] PositionUpdateDto dto)
    {
        var position = await _identityContext.Positions.FindAsync(id);
        if (position == null || position.IsDeleted)
            return NotFound();

        if (!string.IsNullOrEmpty(dto.PositionCode) && dto.PositionCode != position.PositionCode)
        {
            if (await _identityContext.Positions.AnyAsync(p => p.PositionCode == dto.PositionCode && p.Id != id && !p.IsDeleted))
                return BadRequest(new { error = "کد پست تکراری است" });
            position.PositionCode = dto.PositionCode;
        }

        if (!string.IsNullOrEmpty(dto.PositionName)) position.PositionName = dto.PositionName;
        if (dto.Description != null) position.Description = dto.Description;
        if (dto.ParentPositionId.HasValue) position.ParentPositionId = dto.ParentPositionId;
        if (dto.OrganizationId.HasValue) position.OrganizationId = dto.OrganizationId;
        if (dto.DepartmentId.HasValue) position.DepartmentId = dto.DepartmentId;
        if (dto.IsActive.HasValue) position.IsActive = dto.IsActive.Value;

        position.EditDate = DateTime.UtcNow;

        if (dto.RoleIds != null)
        {
            var existing = await _identityContext.RolePositions
                .Where(rp => rp.PositionId == id && !rp.IsDeleted)
                .ToListAsync();
            foreach (var rp in existing) rp.IsDeleted = true;
            foreach (var roleId in dto.RoleIds.Distinct())
                _identityContext.RolePositions.Add(new RolePosition { RoleId = roleId, PositionId = id });
        }

        if (dto.UserIds != null)
        {
            var existingUsers = await _identityContext.UserPositions
                .Where(up => up.PositionId == id && !up.IsDeleted)
                .ToListAsync();
            foreach (var up in existingUsers) up.IsDeleted = true;
            foreach (var userId in dto.UserIds.Distinct())
                _identityContext.UserPositions.Add(new UserPosition { UserId = userId, PositionId = id, IsPrimary = false });
        }

        await _identityContext.SaveChangesAsync();
        return Json(new { success = true });
    }

    [HttpDelete]
    [Route("api/settings/positions/{id}")]
    public async Task<IActionResult> DeletePosition(int id)
    {
        var position = await _identityContext.Positions.FindAsync(id);
        if (position == null || position.IsDeleted)
            return NotFound();

        position.IsDeleted = true;
        position.EditDate = DateTime.UtcNow;

        var roleLinks = await _identityContext.RolePositions.Where(rp => rp.PositionId == id && !rp.IsDeleted).ToListAsync();
        var userLinks = await _identityContext.UserPositions.Where(up => up.PositionId == id && !up.IsDeleted).ToListAsync();
        foreach (var rp in roleLinks) rp.IsDeleted = true;
        foreach (var up in userLinks) up.IsDeleted = true;

        await _identityContext.SaveChangesAsync();
        return Json(new { success = true });
    }

    // ==========================================
    //           EMAIL SETTINGS
    // ==========================================

    [HttpGet]
    [Route("settings/email-settings")]
    public IActionResult EmailSettings()
    {
        return View();
    }

    // ==========================================
    //           FAX SETTINGS
    // ==========================================

    [HttpGet]
    [Route("settings/fax-settings")]
    public IActionResult FaxSettings()
    {
        return View();
    }
}

#region DTOs

public class UserCreateDto
{
    public string? Title { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? FatherName { get; set; }
    public string? NationalId { get; set; }
    public string? Gender { get; set; }
    public string? MaritalStatus { get; set; }
    public string? PersonnelNumber { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Notes { get; set; }
    public string? AvatarPath { get; set; }
    public int? DepartmentId { get; set; }
    public int? OrganizationId { get; set; }
    public bool? IsActive { get; set; }
    public List<int>? RoleIds { get; set; }
    public List<UserSignatureDto>? Signatures { get; set; }
}

public class UserUpdateDto
{
    public string? Title { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FatherName { get; set; }
    public string? NationalId { get; set; }
    public string? Gender { get; set; }
    public string? MaritalStatus { get; set; }
    public string? PersonnelNumber { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Notes { get; set; }
    public string? AvatarPath { get; set; }
    public int? DepartmentId { get; set; }
    public int? OrganizationId { get; set; }
    public bool? IsActive { get; set; }
    public List<int>? RoleIds { get; set; }
    public List<UserSignatureDto>? Signatures { get; set; }
}

public class UserSignatureDto
{
    public int? Id { get; set; }
    public string SignatureTitle { get; set; } = string.Empty;
    public string SignaturePath { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;
    public bool IsUsed { get; set; } = false;
}

public class RoleCreateDto
{
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentRoleId { get; set; }
    public int? OrganizationId { get; set; }
    public bool? IsActive { get; set; }
    public string? CorrespondenceTitle { get; set; }
    public string? DisplayTitle { get; set; }
    public int? PersonnelUserId { get; set; }
    public int? DepartmentId { get; set; }
    public List<int>? PermissionIds { get; set; }
    public List<int>? GroupIds { get; set; }
}

public class RoleUpdateDto
{
    public string? RoleCode { get; set; }
    public string? RoleName { get; set; }
    public string? Description { get; set; }
    public int? ParentRoleId { get; set; }
    public int? OrganizationId { get; set; }
    public bool? IsActive { get; set; }
    public string? CorrespondenceTitle { get; set; }
    public string? DisplayTitle { get; set; }
    public int? PersonnelUserId { get; set; }
    public int? DepartmentId { get; set; }
    public List<int>? PermissionIds { get; set; }
    public List<int>? GroupIds { get; set; }
}

public class PositionCreateDto
{
    public string PositionCode { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentPositionId { get; set; }
    public int? OrganizationId { get; set; }
    public int? DepartmentId { get; set; }
    public bool? IsActive { get; set; }
    public List<int>? RoleIds { get; set; }
    public List<int>? UserIds { get; set; }
}

public class PositionUpdateDto
{
    public string? PositionCode { get; set; }
    public string? PositionName { get; set; }
    public string? Description { get; set; }
    public int? ParentPositionId { get; set; }
    public int? OrganizationId { get; set; }
    public int? DepartmentId { get; set; }
    public bool? IsActive { get; set; }
    public List<int>? RoleIds { get; set; }
    public List<int>? UserIds { get; set; }
}

public class GroupCreateDto
{
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentGroupId { get; set; }
    public bool UseInReferral { get; set; }
    public bool? IsActive { get; set; }
}

public class GroupUpdateDto
{
    public string? GroupCode { get; set; }
    public string? GroupName { get; set; }
    public string? Description { get; set; }
    public int? ParentGroupId { get; set; }
    public bool? UseInReferral { get; set; }
    public bool? IsActive { get; set; }
}

public class OrganizationCreateDto
{
    public string OrganizationCode { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public bool IsParentOrganization { get; set; } = false;
    public bool IsSubsidiary { get; set; } = false;
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class OrganizationUpdateDto
{
    public string? OrganizationCode { get; set; }
    public string? OrganizationName { get; set; }
    public bool? IsParentOrganization { get; set; }
    public bool? IsSubsidiary { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class DepartmentCreateDto
{
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? OrganizationId { get; set; }
    public int? ParentDepartmentId { get; set; }
    public bool? IsActive { get; set; }
}

public class DepartmentUpdateDto
{
    public string? DepartmentCode { get; set; }
    public string? DepartmentName { get; set; }
    public string? Description { get; set; }
    public int? OrganizationId { get; set; }
    public int? ParentDepartmentId { get; set; }
    public bool? IsActive { get; set; }
}

public class PermissionCreateDto
{
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
}

public class PermissionUpdateDto
{
    public string? PermissionCode { get; set; }
    public string? PermissionName { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
}

#endregion




