using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Automation.Infrastructure.Services.Audit;

/// <summary>
/// سرویس ردیابی و لاگ‌نویسی
/// </summary>
public class AuditTrailService
{
    private readonly AutomationDbContext _context;

    public AuditTrailService(AutomationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// ثبت فعالیت کاربر
    /// </summary>
    public async Task LogUserActivityAsync(UserActivityLog logEntry)
    {
        _context.Set<UserActivityLog>().Add(logEntry);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// ثبت تغییر در موجودیت
    /// </summary>
    public async Task LogEntityChangeAsync<T>(T oldEntity, T newEntity, int userId, string action) where T : class
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            EntityType = typeof(T).Name,
            EntityId = GetEntityId(oldEntity ?? newEntity),
            Action = action,
            OldValue = oldEntity != null ? JsonSerializer.Serialize(oldEntity) : null,
            NewValue = newEntity != null ? JsonSerializer.Serialize(newEntity) : null,
            Timestamp = DateTime.UtcNow,
            IpAddress = GetCurrentUserIpAddress(),
            UserAgent = GetCurrentUserAgent()
        };

        _context.Set<AuditLog>().Add(auditLog);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// دریافت لاگ‌های کاربر
    /// </summary>
    public async Task<List<UserActivityLog>> GetUserActivityLogsAsync(int userId, DateTime fromDate, DateTime toDate)
    {
        return await _context.Set<UserActivityLog>()
            .Where(log => log.UserId == userId && 
                         log.Timestamp >= fromDate && 
                         log.Timestamp <= toDate)
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync();
    }

    /// <summary>
    /// دریافت لاگ‌های سیستمی
    /// </summary>
    public async Task<List<SystemLog>> GetSystemLogsAsync(DateTime fromDate, DateTime toDate, string logLevel = null)
    {
        var query = _context.Set<SystemLog>()
            .Where(log => log.Timestamp >= fromDate && log.Timestamp <= toDate);

        if (!string.IsNullOrEmpty(logLevel))
        {
            query = query.Where(log => log.Level == logLevel);
        }

        return await query
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync();
    }

    /// <summary>
    /// ثبت خطا در سیستم
    /// </summary>
    public async Task LogErrorAsync(Exception exception, string context, int? userId = null)
    {
        var errorLog = new ErrorLog
        {
            UserId = userId,
            ExceptionType = exception.GetType().FullName,
            Message = exception.Message,
            StackTrace = exception.StackTrace,
            Context = context,
            Timestamp = DateTime.UtcNow,
            IpAddress = GetCurrentUserIpAddress(),
            UserAgent = GetCurrentUserAgent()
        };

        _context.Set<ErrorLog>().Add(errorLog);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// ثبت عملیات امنیتی
    /// </summary>
    public async Task LogSecurityEventAsync(SecurityLog logEntry)
    {
        _context.Set<SecurityLog>().Add(logEntry);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// گزارش فعالیت‌های مشکوک
    /// </summary>
    public async Task<List<SuspiciousActivityReport>> GetSuspiciousActivitiesAsync(DateTime fromDate, DateTime toDate)
    {
        var failedLogins = await _context.Set<SecurityLog>()
            .Where(sl => sl.EventType == "FAILED_LOGIN" && 
                        sl.Timestamp >= fromDate && 
                        sl.Timestamp <= toDate)
            .GroupBy(sl => sl.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                FailedAttempts = g.Count(),
                LastAttempt = g.Max(x => x.Timestamp)
            })
            .Where(g => g.FailedAttempts > 5) // بیش از 5 تلاش ناموفق
            .ToListAsync();

        var suspiciousReports = failedLogins.Select(f => new SuspiciousActivityReport
        {
            UserId = f.UserId,
            ActivityType = "FAILED_LOGIN_ATTEMPTS",
            Count = f.FailedAttempts,
            LastOccurrence = f.LastAttempt,
            Severity = f.FailedAttempts > 10 ? "HIGH" : "MEDIUM"
        }).ToList();

        return suspiciousReports;
    }

    /// <summary>
    /// گزارش دسترسی‌های غیرمجاز
    /// </summary>
    public async Task<List<UnauthorizedAccessReport>> GetUnauthorizedAccessAttemptsAsync(DateTime fromDate, DateTime toDate)
    {
        var unauthorizedAccesses = await _context.Set<SecurityLog>()
            .Where(sl => sl.EventType == "UNAUTHORIZED_ACCESS" && 
                        sl.Timestamp >= fromDate && 
                        sl.Timestamp <= toDate)
            .GroupBy(sl => new { sl.UserId, sl.Resource })
            .Select(g => new UnauthorizedAccessReport
            {
                UserId = g.Key.UserId,
                Resource = g.Key.Resource,
                AttemptCount = g.Count(),
                FirstAttempt = g.Min(x => x.Timestamp),
                LastAttempt = g.Max(x => x.Timestamp)
            })
            .ToListAsync();

        return unauthorizedAccesses;
    }

    /// <summary>
    /// گزارش تغییرات مهم
    /// </summary>
    public async Task<List<ImportantChangeReport>> GetImportantChangesAsync(DateTime fromDate, DateTime toDate)
    {
        var importantChanges = await _context.Set<AuditLog>()
            .Where(al => (al.EntityType == "User" || al.EntityType == "Role" || al.EntityType == "Permission") &&
                        al.Timestamp >= fromDate && 
                        al.Timestamp <= toDate)
            .Select(al => new ImportantChangeReport
            {
                EntityType = al.EntityType,
                EntityId = al.EntityId,
                Action = al.Action,
                UserId = al.UserId,
                Timestamp = al.Timestamp,
                Details = al.NewValue
            })
            .ToListAsync();

        return importantChanges;
    }

    /// <summary>
    /// پاک‌سازی لاگ‌های قدیمی
    /// </summary>
    public async Task CleanupOldLogsAsync(int daysToKeep = 90)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

        // پاک‌سازی لاگ‌های کاربر
        var oldUserLogs = await _context.Set<UserActivityLog>()
            .Where(log => log.Timestamp < cutoffDate)
            .ToListAsync();
        _context.Set<UserActivityLog>().RemoveRange(oldUserLogs);

        // پاک‌سازی لاگ‌های سیستمی
        var oldSystemLogs = await _context.Set<SystemLog>()
            .Where(log => log.Timestamp < cutoffDate)
            .ToListAsync();
        _context.Set<SystemLog>().RemoveRange(oldSystemLogs);

        // پاک‌سازی لاگ‌های خطا
        var oldErrorLogs = await _context.Set<ErrorLog>()
            .Where(log => log.Timestamp < cutoffDate)
            .ToListAsync();
        _context.Set<ErrorLog>().RemoveRange(oldErrorLogs);

        // پاک‌سازی لاگ‌های امنیتی
        var oldSecurityLogs = await _context.Set<SecurityLog>()
            .Where(log => log.Timestamp < cutoffDate)
            .ToListAsync();
        _context.Set<SecurityLog>().RemoveRange(oldSecurityLogs);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// دریافت آمار استفاده
    /// </summary>
    public async Task<UsageStatistics> GetUsageStatisticsAsync(DateTime fromDate, DateTime toDate)
    {
        var activeUsers = await _context.Set<UserActivityLog>()
            .Where(log => log.Timestamp >= fromDate && log.Timestamp <= toDate)
            .Select(log => log.UserId)
            .Distinct()
            .CountAsync();

        var totalActivities = await _context.Set<UserActivityLog>()
            .CountAsync(log => log.Timestamp >= fromDate && log.Timestamp <= toDate);

        var documentCreations = await _context.Set<UserActivityLog>()
            .CountAsync(log => log.Timestamp >= fromDate && log.Timestamp <= toDate && 
                              log.ActivityType == "DOCUMENT_CREATE");

        var workflowExecutions = await _context.Set<UserActivityLog>()
            .CountAsync(log => log.Timestamp >= fromDate && log.Timestamp <= toDate && 
                              log.ActivityType == "WORKFLOW_EXECUTE");

        return new UsageStatistics
        {
            ActiveUsers = activeUsers,
            TotalActivities = totalActivities,
            DocumentCreations = documentCreations,
            WorkflowExecutions = workflowExecutions,
            PeriodStart = fromDate,
            PeriodEnd = toDate
        };
    }

    /// <summary>
    /// دریافت شناسه موجودیت
    /// </summary>
    private int GetEntityId<T>(T entity) where T : class
    {
        // این روش نیاز به بهبود دارد برای انواع مختلف موجودیت‌ها
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null)
        {
            return (int)idProperty.GetValue(entity);
        }
        return 0;
    }

    /// <summary>
    /// دریافت آدرس IP کاربر فعلی
    /// </summary>
    private string GetCurrentUserIpAddress()
    {
        // در محیط واقعی این مقدار از HttpContext گرفته می‌شود
        return "127.0.0.1";
    }

    /// <summary>
    /// دریافت User Agent کاربر
    /// </summary>
    private string GetCurrentUserAgent()
    {
        // در محیط واقعی این مقدار از HttpContext گرفته می‌شود
        return "Unknown";
    }
}

/// <summary>
/// آمار استفاده
/// </summary>
public class UsageStatistics
{
    public int ActiveUsers { get; set; }
    public int TotalActivities { get; set; }
    public int DocumentCreations { get; set; }
    public int WorkflowExecutions { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

/// <summary>
/// گزارش فعالیت‌های مشکوک
/// </summary>
public class SuspiciousActivityReport
{
    public int? UserId { get; set; }
    public string ActivityType { get; set; }
    public int Count { get; set; }
    public DateTime LastOccurrence { get; set; }
    public string Severity { get; set; }
}

/// <summary>
/// گزارش دسترسی‌های غیرمجاز
/// </summary>
public class UnauthorizedAccessReport
{
    public int? UserId { get; set; }
    public string Resource { get; set; }
    public int AttemptCount { get; set; }
    public DateTime FirstAttempt { get; set; }
    public DateTime LastAttempt { get; set; }
}

/// <summary>
/// گزارش تغییرات مهم
/// </summary>
public class ImportantChangeReport
{
    public string EntityType { get; set; }
    public int EntityId { get; set; }
    public string Action { get; set; }
    public int? UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Details { get; set; }
}