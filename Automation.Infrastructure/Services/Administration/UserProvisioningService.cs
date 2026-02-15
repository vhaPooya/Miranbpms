using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Automation.Infrastructure.Services.Administration;

/// <summary>
/// سرویس اتوماسیون تأمین کاربران برای مدیریت خودکار حساب‌های کاربری
/// </summary>
public class UserProvisioningService
{
    private readonly AutomationDbContext _context;
    private readonly ILogger<UserProvisioningService> _logger;

    public UserProvisioningService(AutomationDbContext context, ILogger<UserProvisioningService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// ایجاد کاربر جدید به صورت خودکار
    /// </summary>
    public async Task<ProvisioningResult> CreateUserAsync(UserProvisioningRequest request)
    {
        try
        {
            // بررسی وجود کاربر با این ایمیل یا نام کاربری
            var existingUser = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == request.Email || u.Username == request.Username);

            if (existingUser != null)
            {
                return new ProvisioningResult
                {
                    Success = false,
                    ErrorMessage = "User already exists",
                    UserId = existingUser.Id
                };
            }

            // ایجاد کاربر جدید
            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Mobile = request.Mobile,
                OrganizationId = request.OrganizationId,
                DepartmentId = request.DepartmentId,
                IsActive = true,
                Created = DateTime.UtcNow,
                LastPasswordChange = DateTime.UtcNow
            };

            // تنظیم کلمه عبور (رمزگذاری شده)
            newUser.PasswordHash = await GeneratePasswordHashAsync(request.InitialPassword ?? GenerateTemporaryPassword());

            _context.Set<User>().Add(newUser);
            await _context.SaveChangesAsync();

            // اختصاص نقش‌ها
            if (request.RoleIds?.Any() == true)
            {
                await AssignRolesAsync(newUser.Id, request.RoleIds);
            }

            // اختصاص سمت‌ها
            if (request.PositionIds?.Any() == true)
            {
                await AssignPositionsAsync(newUser.Id, request.PositionIds);
            }

            // ایجاد تنظیمات پیش‌فرض
            await CreateDefaultSettingsAsync(newUser.Id);

            _logger.LogInformation($"User provisioned successfully: {newUser.Username} ({newUser.Id})");
            
            return new ProvisioningResult
            {
                Success = true,
                UserId = newUser.Id,
                Message = "User created successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to provision user: {request.Username}");
            return new ProvisioningResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// به‌روزرسانی کاربر به صورت خودکار
    /// </summary>
    public async Task<ProvisioningResult> UpdateUserAsync(int userId, UserUpdateRequest request)
    {
        try
        {
            var user = await _context.Set<User>().FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            
            if (user == null)
            {
                return new ProvisioningResult
                {
                    Success = false,
                    ErrorMessage = "User not found"
                };
            }

            // به‌روزرسانی اطلاعات
            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.Email = request.Email ?? user.Email;
            user.Mobile = request.Mobile ?? user.Mobile;
            user.OrganizationId = request.OrganizationId ?? user.OrganizationId;
            user.DepartmentId = request.DepartmentId ?? user.DepartmentId;
            user.IsActive = request.IsActive ?? user.IsActive;
            user.Modified = DateTime.UtcNow;

            // تغییر کلمه عبور در صورت درخواست
            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                user.PasswordHash = await GeneratePasswordHashAsync(request.NewPassword);
                user.LastPasswordChange = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // به‌روزرسانی نقش‌ها
            if (request.RoleIds != null)
            {
                await UpdateUserRolesAsync(userId, request.RoleIds);
            }

            // به‌روزرسانی سمت‌ها
            if (request.PositionIds != null)
            {
                await UpdateUserPositionsAsync(userId, request.PositionIds);
            }

            _logger.LogInformation($"User updated successfully: {user.Username} ({userId})");
            
            return new ProvisioningResult
            {
                Success = true,
                UserId = userId,
                Message = "User updated successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to update user: {userId}");
            return new ProvisioningResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// حذف کاربر به صورت خودکار
    /// </summary>
    public async Task<ProvisioningResult> DeleteUserAsync(int userId, bool permanent = false)
    {
        try
        {
            var user = await _context.Set<User>().FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user == null)
            {
                return new ProvisioningResult
                {
                    Success = false,
                    ErrorMessage = "User not found"
                };
            }

            if (permanent)
            {
                // حذف دائمی
                _context.Set<User>().Remove(user);
                
                // حذف موارد مرتبط
                await RemoveRelatedDataAsync(userId);
            }
            else
            {
                // حذف منطقی
                user.IsDeleted = true;
                user.Deleted = DateTime.UtcNow;
                user.IsActive = false;
            }

            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"User {(permanent ? "permanently deleted" : "deactivated")}: {user.Username} ({userId})");
            
            return new ProvisioningResult
            {
                Success = true,
                UserId = userId,
                Message = $"User {(permanent ? "permanently deleted" : "deactivated")} successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to delete user: {userId}");
            return new ProvisioningResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// اختصاص نقش‌ها به کاربر
    /// </summary>
    private async Task AssignRolesAsync(int userId, List<int> roleIds)
    {
        foreach (var roleId in roleIds)
        {
            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                IsActive = true,
                AssignedDate = DateTime.UtcNow
            };

            _context.Set<UserRole>().Add(userRole);
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// اختصاص سمت‌ها به کاربر
    /// </summary>
    private async Task AssignPositionsAsync(int userId, List<int> positionIds)
    {
        foreach (var positionId in positionIds)
        {
            var userPosition = new UserPosition
            {
                UserId = userId,
                PositionId = positionId,
                IsActive = true
            };

            _context.Set<UserPosition>().Add(userPosition);
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// به‌روزرسانی نقش‌های کاربر
    /// </summary>
    private async Task UpdateUserRolesAsync(int userId, List<int> roleIds)
    {
        // حذف نقش‌های قبلی
        var existingRoles = await _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .ToListAsync();
        
        _context.Set<UserRole>().RemoveRange(existingRoles);

        // اضافه کردن نقش‌های جدید
        await AssignRolesAsync(userId, roleIds);
    }

    /// <summary>
    /// به‌روزرسانی سمت‌های کاربر
    /// </summary>
    private async Task UpdateUserPositionsAsync(int userId, List<int> positionIds)
    {
        // حذف سمت‌های قبلی
        var existingPositions = await _context.Set<UserPosition>()
            .Where(up => up.UserId == userId)
            .ToListAsync();
        
        _context.Set<UserPosition>().RemoveRange(existingPositions);

        // اضافه کردن سمت‌های جدید
        await AssignPositionsAsync(userId, positionIds);
    }

    /// <summary>
    /// ایجاد تنظیمات پیش‌فرض برای کاربر
    /// </summary>
    private async Task CreateDefaultSettingsAsync(int userId)
    {
        var defaultSettings = new List<UserSetting>
        {
            new UserSetting
            {
                UserId = userId,
                SettingKey = "theme",
                SettingValue = "light",
                Category = "appearance"
            },
            new UserSetting
            {
                UserId = userId,
                SettingKey = "language",
                SettingValue = "fa",
                Category = "localization"
            },
            new UserSetting
            {
                UserId = userId,
                SettingKey = "notifications_email",
                SettingValue = "true",
                Category = "notifications"
            },
            new UserSetting
            {
                UserId = userId,
                SettingKey = "notifications_sms",
                SettingValue = "false",
                Category = "notifications"
            }
        };

        _context.Set<UserSetting>().AddRange(defaultSettings);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// حذف داده‌های مرتبط با کاربر
    /// </summary>
    private async Task RemoveRelatedDataAsync(int userId)
    {
        // حذف اعلان‌ها
        var notifications = await _context.Set<Notification>()
            .Where(n => n.UserId == userId)
            .ToListAsync();
        _context.Set<Notification>().RemoveRange(notifications);

        // حذف لاگ‌های فعالیت
        var activityLogs = await _context.Set<UserActivityLog>()
            .Where(l => l.UserId == userId)
            .ToListAsync();
        _context.Set<UserActivityLog>().RemoveRange(activityLogs);

        // حذف تنظیمات کاربر
        var userSettings = await _context.Set<UserSetting>()
            .Where(s => s.UserId == userId)
            .ToListAsync();
        _context.Set<UserSetting>().RemoveRange(userSettings);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// تولید کلمه عبور موقت
    /// </summary>
    private string GenerateTemporaryPassword(int length = 12)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        var password = new char[length];
        
        for (int i = 0; i < length; i++)
        {
            password[i] = chars[random.Next(chars.Length)];
        }
        
        return new string(password);
    }

    /// <summary>
    /// رمزگذاری کلمه عبور
    /// </summary>
    private async Task<string> GeneratePasswordHashAsync(string password)
    {
        // در اینجا باید از سرویس رمزگذاری واقعی استفاده کنیم
        // برای سادگی یک هش ساده ایجاد می‌کنیم
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// اتوماسیون تأمین گروهی کاربران
    /// </summary>
    public async Task<BulkProvisioningResult> BulkProvisionUsersAsync(List<UserProvisioningRequest> requests)
    {
        var results = new List<ProvisioningResult>();
        var successCount = 0;
        var failureCount = 0;

        foreach (var request in requests)
        {
            try
            {
                var result = await CreateUserAsync(request);
                results.Add(result);
                
                if (result.Success)
                    successCount++;
                else
                    failureCount++;
            }
            catch (Exception ex)
            {
                results.Add(new ProvisioningResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
                failureCount++;
            }
        }

        return new BulkProvisioningResult
        {
            Success = failureCount == 0,
            TotalProcessed = requests.Count,
            SuccessCount = successCount,
            FailureCount = failureCount,
            Results = results,
            CompletedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// دریافت کاربران بر اساس معیارهای مختلف
    /// </summary>
    public async Task<List<UserProvisioningInfo>> GetUsersAsync(UserSearchCriteria criteria)
    {
        var query = _context.Set<User>()
            .Include(u => u.Organization)
            .Include(u => u.Department)
            .AsQueryable();

        // فیلتر بر اساس نام
        if (!string.IsNullOrEmpty(criteria.NameFilter))
        {
            query = query.Where(u => u.FirstName.Contains(criteria.NameFilter) || 
                                   u.LastName.Contains(criteria.NameFilter) ||
                                   u.Username.Contains(criteria.NameFilter));
        }

        // فیلتر بر اساس سازمان
        if (criteria.OrganizationId.HasValue)
        {
            query = query.Where(u => u.OrganizationId == criteria.OrganizationId.Value);
        }

        // فیلتر بر اساس وضعیت
        if (criteria.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == criteria.IsActive.Value);
        }

        // فیلتر بر اساس تاریخ ایجاد
        if (criteria.CreatedAfter.HasValue)
        {
            query = query.Where(u => u.Created >= criteria.CreatedAfter.Value);
        }

        if (criteria.CreatedBefore.HasValue)
        {
            query = query.Where(u => u.Created <= criteria.CreatedBefore.Value);
        }

        // صفحه‌بندی
        query = query.Skip((criteria.Page - 1) * criteria.PageSize)
                     .Take(criteria.PageSize);

        var users = await query.ToListAsync();

        return users.Select(u => new UserProvisioningInfo
        {
            UserId = u.Id,
            Username = u.Username,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            OrganizationName = u.Organization?.OrganizationName,
            DepartmentName = u.Department?.DepartmentName,
            IsActive = u.IsActive,
            CreatedDate = u.Created,
            LastLoginDate = u.LastLoginDate
        }).ToList();
    }

    /// <summary>
    /// فعال‌سازی/غیرفعال‌سازی کاربر
    /// </summary>
    public async Task<ProvisioningResult> ToggleUserStatusAsync(int userId, bool isActive)
    {
        try
        {
            var user = await _context.Set<User>().FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user == null)
            {
                return new ProvisioningResult
                {
                    Success = false,
                    ErrorMessage = "User not found"
                };
            }

            user.IsActive = isActive;
            user.Modified = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"User status toggled: {user.Username} ({userId}) - Active: {isActive}");
            
            return new ProvisioningResult
            {
                Success = true,
                UserId = userId,
                Message = $"User {(isActive ? "activated" : "deactivated")} successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to toggle user status: {userId}");
            return new ProvisioningResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// راه‌اندازی اولیه سیستم
    /// </summary>
    public async Task<ProvisioningResult> InitializeSystemAsync(SystemInitializationRequest request)
    {
        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            // ایجاد سازمان اصلی
            var organization = new Organization
            {
                OrganizationCode = request.OrganizationCode,
                OrganizationName = request.OrganizationName,
                Description = "سازمان اصلی سیستم"
            };
            _context.Set<Organization>().Add(organization);
            await _context.SaveChangesAsync();

            // ایجاد بخش‌های اصلی
            foreach (var deptName in request.Departments)
            {
                var department = new Department
                {
                    DepartmentCode = deptName.ToUpper().Replace(" ", "_"),
                    DepartmentName = deptName,
                    OrganizationId = organization.Id,
                    Description = $"بخش {deptName}"
                };
                _context.Set<Department>().Add(department);
            }
            await _context.SaveChangesAsync();

            // ایجاد نقش‌های پیش‌فرض
            var roles = new List<Role>
            {
                new Role
                {
                    RoleCode = "ADMIN",
                    RoleName = "مدیر سیستم",
                    OrganizationId = organization.Id,
                    Description = "نقش مدیریتی با دسترسی کامل"
                },
                new Role
                {
                    RoleCode = "USER",
                    RoleName = "کاربر عادی",
                    OrganizationId = organization.Id,
                    Description = "نقش کاربری عادی"
                }
            };
            _context.Set<Role>().AddRange(roles);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogInformation("System initialized successfully");
            
            return new ProvisioningResult
            {
                Success = true,
                Message = "System initialized successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize system");
            return new ProvisioningResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}

/// <summary>
/// درخواست تأمین کاربر
/// </summary>
public class UserProvisioningRequest
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Mobile { get; set; }
    public int OrganizationId { get; set; }
    public int? DepartmentId { get; set; }
    public string InitialPassword { get; set; }
    public List<int> RoleIds { get; set; } = new List<int>();
    public List<int> PositionIds { get; set; } = new List<int>();
}

/// <summary>
/// درخواست به‌روزرسانی کاربر
/// </summary>
public class UserUpdateRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
    public int? OrganizationId { get; set; }
    public int? DepartmentId { get; set; }
    public bool? IsActive { get; set; }
    public string NewPassword { get; set; }
    public List<int> RoleIds { get; set; }
    public List<int> PositionIds { get; set; }
}

/// <summary>
/// نتیجه تأمین کاربر
/// </summary>
public class ProvisioningResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string ErrorMessage { get; set; }
    public int? UserId { get; set; }
}

/// <summary>
/// نتیجه تأمین گروهی
/// </summary>
public class BulkProvisioningResult
{
    public bool Success { get; set; }
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<ProvisioningResult> Results { get; set; } = new List<ProvisioningResult>();
    public DateTime CompletedAt { get; set; }
}

/// <summary>
/// اطلاعات کاربر برای تأمین
/// </summary>
public class UserProvisioningInfo
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string OrganizationName { get; set; }
    public string DepartmentName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
}

/// <summary>
/// معیارهای جستجوی کاربر
/// </summary>
public class UserSearchCriteria
{
    public string NameFilter { get; set; }
    public int? OrganizationId { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// درخواست راه‌اندازی اولیه سیستم
/// </summary>
public class SystemInitializationRequest
{
    public string OrganizationCode { get; set; }
    public string OrganizationName { get; set; }
    public List<string> Departments { get; set; } = new List<string>();
    public List<string> DefaultRoles { get; set; } = new List<string>();
}