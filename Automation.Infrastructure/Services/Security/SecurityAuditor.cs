using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Automation.Infrastructure.Services.Security;

/// <summary>
/// سیستم ممیزی امنیتی برای ردیابی و نظارت بر فعالیت‌های امنیتی
/// </summary>
public class SecurityAuditor
{
    private readonly AutomationDbContext _context;
    private readonly ILogger<SecurityAuditor> _logger;

    public SecurityAuditor(AutomationDbContext context, ILogger<SecurityAuditor> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// ثبت فعالیت امنیتی
    /// </summary>
    public async Task LogSecurityEventAsync(SecurityEvent securityEvent)
    {
        try
        {
            var log = new Automation.Core.Entities.SecurityLog
            {
                EventType = securityEvent.EventType ?? "",
                UserId = securityEvent.UserId,
                Timestamp = securityEvent.Timestamp,
                Details = securityEvent.Description,
                IpAddress = securityEvent.IpAddress
            };
            _context.Set<SecurityLog>().Add(log);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Security event logged: {securityEvent.EventType} - {securityEvent.UserId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log security event");
        }
    }

    /// <summary>
    /// ثبت ورود موفق
    /// </summary>
    public async Task LogSuccessfulLoginAsync(int userId, string ipAddress, string userAgent)
    {
        var securityEvent = new SecurityEvent
        {
            UserId = userId,
            EventType = "SUCCESSFUL_LOGIN",
            Description = "User logged in successfully",
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow,
            Severity = SecurityEventSeverity.Info
        };

        await LogSecurityEventAsync(securityEvent);
    }

    /// <summary>
    /// ثبت ورود ناموفق
    /// </summary>
    public async Task LogFailedLoginAsync(int? userId, string username, string ipAddress, string userAgent, string reason = null)
    {
        var securityEvent = new SecurityEvent
        {
            UserId = userId,
            EventType = "FAILED_LOGIN",
            Description = $"Failed login attempt for user '{username}'. Reason: {reason}",
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow,
            Severity = SecurityEventSeverity.Warning
        };

        await LogSecurityEventAsync(securityEvent);
    }

    /// <summary>
    /// ثبت دسترسی غیرمجاز
    /// </summary>
    public async Task LogUnauthorizedAccessAsync(int userId, string resource, string action, string ipAddress, string userAgent)
    {
        var securityEvent = new SecurityEvent
        {
            UserId = userId,
            EventType = "UNAUTHORIZED_ACCESS",
            Description = $"Unauthorized access attempt to resource '{resource}' with action '{action}'",
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow,
            Severity = SecurityEventSeverity.High
        };

        await LogSecurityEventAsync(securityEvent);
    }

    /// <summary>
    /// ثبت تغییر کلمه عبور
    /// </summary>
    public async Task LogPasswordChangeAsync(int userId, string ipAddress, string userAgent)
    {
        var securityEvent = new SecurityEvent
        {
            UserId = userId,
            EventType = "PASSWORD_CHANGED",
            Description = "User changed password",
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow,
            Severity = SecurityEventSeverity.Info
        };

        await LogSecurityEventAsync(securityEvent);
    }

    /// <summary>
    /// ثبت فعال‌سازی 2FA
    /// </summary>
    public async Task LogTwoFactorEnabledAsync(int userId, TwoFactorMethod method, string ipAddress, string userAgent)
    {
        var securityEvent = new SecurityEvent
        {
            UserId = userId,
            EventType = "TWO_FACTOR_ENABLED",
            Description = $"Two-factor authentication enabled with method: {method}",
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow,
            Severity = SecurityEventSeverity.Info
        };

        await LogSecurityEventAsync(securityEvent);
    }

    /// <summary>
    /// ثبت غیرفعال‌سازی 2FA
    /// </summary>
    public async Task LogTwoFactorDisabledAsync(int userId, string ipAddress, string userAgent)
    {
        var securityEvent = new SecurityEvent
        {
            UserId = userId,
            EventType = "TWO_FACTOR_DISABLED",
            Description = "Two-factor authentication disabled",
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow,
            Severity = SecurityEventSeverity.Warning
        };

        await LogSecurityEventAsync(securityEvent);
    }

    /// <summary>
    /// ثبت استفاده از کد 2FA
    /// </summary>
    public async Task LogTwoFactorUsedAsync(int userId, TwoFactorMethod method, bool success, string ipAddress, string userAgent)
    {
        var securityEvent = new SecurityEvent
        {
            UserId = userId,
            EventType = "TWO_FACTOR_USED",
            Description = $"Two-factor authentication {(success ? "succeeded" : "failed")} with method: {method}",
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow,
            Severity = success ? SecurityEventSeverity.Info : SecurityEventSeverity.Warning
        };

        await LogSecurityEventAsync(securityEvent);
    }

    /// <summary>
    /// ثبت دسترسی به داده‌های حساس
    /// </summary>
    public async Task LogSensitiveDataAccessAsync(int userId, string dataType, string action, string ipAddress, string userAgent)
    {
        var securityEvent = new SecurityEvent
        {
            UserId = userId,
            EventType = "SENSITIVE_DATA_ACCESS",
            Description = $"Accessed sensitive data type '{dataType}' with action '{action}'",
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow,
            Severity = SecurityEventSeverity.High
        };

        await LogSecurityEventAsync(securityEvent);
    }

    /// <summary>
    /// تحلیل فعالیت‌های مشکوک
    /// </summary>
    public async Task<List<SuspiciousActivity>> AnalyzeSuspiciousActivitiesAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var suspiciousActivities = new List<SuspiciousActivity>();

            // تحلیل تلاش‌های ورود ناموفق
            var failedLogins = await _context.Set<SecurityLog>()
                .Where(se => se.EventType == "FAILED_LOGIN" && 
                            se.Timestamp >= fromDate && 
                            se.Timestamp <= toDate)
                .GroupBy(se => new { se.UserId, se.IpAddress })
                .Select(g => new
                {
                    UserId = g.Key.UserId,
                    IpAddress = g.Key.IpAddress,
                    Count = g.Count(),
                    LastAttempt = g.Max(x => x.Timestamp)
                })
                .Where(g => g.Count > 5) // بیش از 5 تلاش ناموفق
                .ToListAsync();

            foreach (var failedLogin in failedLogins)
            {
                suspiciousActivities.Add(new SuspiciousActivity
                {
                    ActivityType = SuspiciousActivityType.FailedLoginAttempts,
                    UserId = failedLogin.UserId,
                    IpAddress = failedLogin.IpAddress,
                    Count = failedLogin.Count,
                    LastOccurrence = failedLogin.LastAttempt,
                    RiskLevel = failedLogin.Count > 10 ? RiskLevel.High : RiskLevel.Medium,
                    Description = $"Multiple failed login attempts ({failedLogin.Count})"
                });
            }

            // تحلیل دسترسی‌های غیرمجاز
            var unauthorizedAccesses = await _context.Set<SecurityLog>()
                .Where(se => se.EventType == "UNAUTHORIZED_ACCESS" && 
                            se.Timestamp >= fromDate && 
                            se.Timestamp <= toDate)
                .GroupBy(se => new { se.UserId, se.Details })
                .Select(g => new
                {
                    UserId = g.Key.UserId,
                    Description = g.Key.Details,
                    Count = g.Count(),
                    LastAttempt = g.Max(x => x.Timestamp)
                })
                .Where(g => g.Count > 3) // بیش از 3 دفعه
                .ToListAsync();

            foreach (var unauthorized in unauthorizedAccesses)
            {
                suspiciousActivities.Add(new SuspiciousActivity
                {
                    ActivityType = SuspiciousActivityType.UnauthorizedAccess,
                    UserId = unauthorized.UserId,
                    Count = unauthorized.Count,
                    LastOccurrence = unauthorized.LastAttempt,
                    RiskLevel = RiskLevel.High,
                    Description = unauthorized.Description
                });
            }

            return suspiciousActivities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze suspicious activities");
            return new List<SuspiciousActivity>();
        }
    }

    /// <summary>
    /// گزارش فعالیت‌های کاربر
    /// </summary>
    public async Task<List<SecurityEvent>> GetUserActivityReportAsync(int userId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var logs = await _context.Set<SecurityLog>()
                .Where(se => se.UserId == userId && 
                            se.Timestamp >= fromDate && 
                            se.Timestamp <= toDate)
                .OrderByDescending(se => se.Timestamp)
                .ToListAsync();
            return logs.Select(se => new SecurityEvent
            {
                UserId = se.UserId,
                EventType = se.EventType,
                Description = se.Details ?? "",
                IpAddress = se.IpAddress ?? "",
                UserAgent = "",
                Timestamp = se.Timestamp,
                Severity = SecurityEventSeverity.Info,
                AdditionalData = ""
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get user activity report for user {userId}");
            return new List<SecurityEvent>();
        }
    }

    /// <summary>
    /// گزارش فعالیت‌های IP
    /// </summary>
    public async Task<List<SecurityEvent>> GetIpActivityReportAsync(string ipAddress, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var logs = await _context.Set<SecurityLog>()
                .Where(se => se.IpAddress == ipAddress && 
                            se.Timestamp >= fromDate && 
                            se.Timestamp <= toDate)
                .OrderByDescending(se => se.Timestamp)
                .ToListAsync();
            return logs.Select(se => new SecurityEvent
            {
                UserId = se.UserId,
                EventType = se.EventType,
                Description = se.Details ?? "",
                IpAddress = se.IpAddress ?? "",
                UserAgent = "",
                Timestamp = se.Timestamp,
                Severity = SecurityEventSeverity.Info,
                AdditionalData = ""
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get IP activity report for {ipAddress}");
            return new List<SecurityEvent>();
        }
    }

    /// <summary>
    /// گزارش کلی فعالیت‌های امنیتی
    /// </summary>
    public async Task<SecurityReport> GenerateSecurityReportAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var report = new SecurityReport
            {
                PeriodStart = fromDate,
                PeriodEnd = toDate,
                GeneratedAt = DateTime.UtcNow
            };

            // تعداد کل فعالیت‌ها
            report.TotalActivities = await _context.Set<SecurityLog>()
                .CountAsync(se => se.Timestamp >= fromDate && se.Timestamp <= toDate);

            // تحلیل انواع فعالیت‌ها
            var eventTypeStatsData = await _context.Set<SecurityLog>()
                .Where(se => se.Timestamp >= fromDate && se.Timestamp <= toDate)
                .GroupBy(se => se.EventType)
                .Select(g => new { EventType = g.Key, Count = g.Count() })
                .ToListAsync();
            var eventTypeStats = eventTypeStatsData.Select(x => new EventTypeStat
            {
                EventType = x.EventType,
                Count = x.Count,
                SeverityDistribution = new Dictionary<string, int> { [x.EventType] = x.Count }
            }).ToList();

            report.EventTypeStats = eventTypeStats;

            // تحلیل شدت رویدادها (بر اساس EventType)
            var severityStats = await _context.Set<SecurityLog>()
                .Where(se => se.Timestamp >= fromDate && se.Timestamp <= toDate)
                .GroupBy(se => se.EventType)
                .Select(g => new SeverityStat
                {
                    Severity = SecurityEventSeverity.Info,
                    SeverityLabel = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            report.SeverityStats = severityStats;

            // کاربران فعال
            report.ActiveUsers = await _context.Set<SecurityLog>()
                .Where(se => se.Timestamp >= fromDate && se.Timestamp <= toDate)
                .Select(se => se.UserId)
                .Distinct()
                .CountAsync();

            // IPهای فعال
            report.ActiveIps = await _context.Set<SecurityLog>()
                .Where(se => se.Timestamp >= fromDate && se.Timestamp <= toDate)
                .Select(se => se.IpAddress)
                .Distinct()
                .CountAsync();

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate security report");
            throw new SecurityException("Failed to generate security report", ex);
        }
    }

    /// <summary>
    /// شناسایی کاربران غیرفعال
    /// </summary>
    public async Task<List<InactiveUser>> FindInactiveUsersAsync(int daysThreshold = 90)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysThreshold);
            
            // کاربرانی که اخیراً فعالیت نداشته‌اند
            var inactiveUsers = await _context.Set<User>()
                .Where(u => u.IsActive && !u.IsDeleted)
                .Where(u => !_context.Set<SecurityLog>()
                    .Any(se => se.UserId == u.Id && se.Timestamp > cutoffDate))
                .Select(u => new InactiveUser
                {
                    UserId = u.Id,
                    Username = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    LastLoginDate = u.LastLoginDate,
                    DaysInactive = (DateTime.UtcNow - (u.LastLoginDate ?? u.CreationDate)).Days
                })
                .ToListAsync();

            return inactiveUsers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find inactive users");
            return new List<InactiveUser>();
        }
    }

    /// <summary>
    /// پاک‌سازی سوابق امنیتی قدیمی
    /// </summary>
    public async Task<int> CleanupOldSecurityLogsAsync(int daysToKeep = 365)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var deletedCount = await _context.Set<SecurityLog>()
                .Where(se => se.Timestamp < cutoffDate)
                .ExecuteDeleteAsync();

            _logger.LogInformation($"Cleaned up {deletedCount} old security logs");
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old security logs");
            return 0;
        }
    }

    /// <summary>
    /// ثبت هشدار امنیتی
    /// </summary>
    public async Task LogSecurityAlertAsync(SecurityAlert alert)
    {
        try
        {
            // ثبت در سیستم اعلان‌ها
            var notification = new Notification
            {
                UserId = alert.TargetUserId ?? 0,
                Title = "هشدار امنیتی",
                Message = alert.Message,
                Type = "SECURITY_ALERT",
                Priority = alert.Severity.ToString(),
                IsRead = false,
                CreatedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            _context.Set<Notification>().Add(notification);
            
            // ثبت در سوابق امنیتی
            var securityLog = new Automation.Core.Entities.SecurityLog
            {
                UserId = alert.TargetUserId,
                EventType = "SECURITY_ALERT",
                Details = $"Security alert: {alert.AlertType} - {alert.Message}",
                Timestamp = DateTime.UtcNow
            };

            _context.Set<SecurityLog>().Add(securityLog);
            
            await _context.SaveChangesAsync();
            
            _logger.LogWarning($"Security alert logged: {alert.AlertType} - {alert.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log security alert");
        }
    }

    /// <summary>
    /// دریافت هشدارهای فعال
    /// </summary>
    public async Task<List<SecurityAlert>> GetActiveSecurityAlertsAsync()
    {
        try
        {
            // در اینجا باید هشدارهای فعال از سیستم را دریافت کنیم
            // برای سادگی، لیست خالی برمی‌گردانیم
            return new List<SecurityAlert>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get active security alerts");
            return new List<SecurityAlert>();
        }
    }
}

/// <summary>
/// رویداد امنیتی
/// </summary>
public class SecurityEvent
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string EventType { get; set; }
    public string Description { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
    public SecurityEventSeverity Severity { get; set; }
    public string AdditionalData { get; set; }
}

/// <summary>
/// سطح شدت رویداد امنیتی
/// </summary>
public enum SecurityEventSeverity
{
    Info,
    Warning,
    High,
    Critical
}

/// <summary>
/// فعالیت مشکوک
/// </summary>
public class SuspiciousActivity
{
    public SuspiciousActivityType ActivityType { get; set; }
    public int? UserId { get; set; }
    public string IpAddress { get; set; }
    public int Count { get; set; }
    public DateTime LastOccurrence { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public string Description { get; set; }
}

/// <summary>
/// انواع فعالیت‌های مشکوک
/// </summary>
public enum SuspiciousActivityType
{
    FailedLoginAttempts,
    UnauthorizedAccess,
    MultiplePasswordChanges,
    SuspiciousDataAccess,
    UnknownIpAddress
}

/// <summary>
/// سطح ریسک
/// </summary>
public enum RiskLevel
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// گزارش امنیتی
/// </summary>
public class SecurityReport
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime GeneratedAt { get; set; }
    public int TotalActivities { get; set; }
    public int ActiveUsers { get; set; }
    public int ActiveIps { get; set; }
    public List<EventTypeStat> EventTypeStats { get; set; } = new List<EventTypeStat>();
    public List<SeverityStat> SeverityStats { get; set; } = new List<SeverityStat>();
}

/// <summary>
/// آمار نوع رویداد
/// </summary>
public class EventTypeStat
{
    public string EventType { get; set; }
    public int Count { get; set; }
    public Dictionary<string, int> SeverityDistribution { get; set; } = new Dictionary<string, int>();
}

/// <summary>
/// آمار شدت
/// </summary>
public class SeverityStat
{
    public SecurityEventSeverity Severity { get; set; }
    public string? SeverityLabel { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// کاربر غیرفعال
/// </summary>
public class InactiveUser
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int DaysInactive { get; set; }
}

/// <summary>
/// هشدار امنیتی
/// </summary>
public class SecurityAlert
{
    public string AlertType { get; set; }
    public int? TargetUserId { get; set; }
    public string Message { get; set; }
    public RiskLevel Severity { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string RecommendedAction { get; set; }
}