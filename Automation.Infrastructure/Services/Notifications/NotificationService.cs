using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Automation.Infrastructure.Services.Notifications;

/// <summary>
/// سرویس مدیریت اعلان‌ها
/// </summary>
public class NotificationService
{
    private readonly AutomationDbContext _context;

    public NotificationService(AutomationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// ایجاد اعلان جدید
    /// </summary>
    public async Task<Notification> CreateNotificationAsync(NotificationRequest request)
    {
        var notification = new Notification
        {
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            ReferenceId = request.ReferenceId,
            ReferenceType = request.ReferenceType,
            Priority = request.Priority,
            IsRead = false,
            CreatedDate = DateTime.UtcNow,
            ExpiryDate = request.ExpiryDate
        };

        _context.Set<Notification>().Add(notification);
        await _context.SaveChangesAsync();

        return notification;
    }

    /// <summary>
    /// دریافت اعلان‌های کاربر
    /// </summary>
    public async Task<List<Notification>> GetUserNotificationsAsync(int userId, int count = 10)
    {
        return await _context.Set<Notification>()
            .Where(n => n.UserId == userId && !n.IsRead && 
                       (!n.ExpiryDate.HasValue || n.ExpiryDate > DateTime.UtcNow))
            .OrderByDescending(n => n.CreatedDate)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// دریافت تعداد اعلان‌های خوانده نشده
    /// </summary>
    public async Task<int> GetUnreadNotificationCountAsync(int userId)
    {
        return await _context.Set<Notification>()
            .CountAsync(n => n.UserId == userId && !n.IsRead &&
                           (!n.ExpiryDate.HasValue || n.ExpiryDate > DateTime.UtcNow));
    }

    /// <summary>
    /// علامت‌گذاری اعلان به عنوان خوانده شده
    /// </summary>
    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.Set<Notification>()
            .FirstOrDefaultAsync(n => n.Id == notificationId);

        if (notification != null)
        {
            notification.IsRead = true;
            notification.ReadDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// علامت‌گذاری همه اعلان‌های کاربر به عنوان خوانده شده
    /// </summary>
    public async Task MarkAllAsReadAsync(int userId)
    {
        var notifications = await _context.Set<Notification>()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// حذف اعلان
    /// </summary>
    public async Task DeleteNotificationAsync(int notificationId)
    {
        var notification = await _context.Set<Notification>()
            .FirstOrDefaultAsync(n => n.Id == notificationId);

        if (notification != null)
        {
            _context.Set<Notification>().Remove(notification);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// ایجاد اعلان گروهی
    /// </summary>
    public async Task CreateBulkNotificationsAsync(List<NotificationRequest> requests)
    {
        var notifications = requests.Select(request => new Notification
        {
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            ReferenceId = request.ReferenceId,
            ReferenceType = request.ReferenceType,
            Priority = request.Priority,
            IsRead = false,
            CreatedDate = DateTime.UtcNow,
            ExpiryDate = request.ExpiryDate
        }).ToList();

        _context.Set<Notification>().AddRange(notifications);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// ایجاد اعلان سیستمی
    /// </summary>
    public async Task CreateSystemNotificationAsync(string title, string message, string type = "SYSTEM")
    {
        // دریافت همه کاربران فعال
        var activeUsers = await _context.Set<User>()
            .Where(u => u.IsActive && !u.IsDeleted)
            .Select(u => u.Id)
            .ToListAsync();

        var notifications = activeUsers.Select(userId => new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Priority = "HIGH",
            IsRead = false,
            CreatedDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        }).ToList();

        _context.Set<Notification>().AddRange(notifications);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// اعلان برای وظایف تاخیر خورده
    /// </summary>
    public async Task CreateOverdueTaskNotificationsAsync()
    {
        var overdueTasks = await _context.Set<TaskAssignment>()
            .Include(ta => ta.WorkflowToken)
                .ThenInclude(wt => wt.WorkflowInstance)
            .Where(ta => ta.Status == "ASSIGNED" && 
                        ta.DueDate < DateTime.UtcNow && 
                        ta.DueDate > DateTime.UtcNow.AddDays(-30)) // فقط 30 روز اخیر
            .ToListAsync();

        foreach (var task in overdueTasks)
        {
            var notification = new Notification
            {
                UserId = task.AssigneeId,
                Title = "وظیفه تاخیر خورده",
                Message = $"وظیفه '{task.WorkflowToken?.WorkflowInstance?.Workflow?.NameFa}' باید در تاریخ {task.DueDate:yyyy/MM/dd} تکمیل شود",
                Type = "TASK_OVERDUE",
                Priority = "HIGH",
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            };

            _context.Set<Notification>().Add(notification);
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// اعلان برای اسناد تاخیر خورده
    /// </summary>
    public async Task CreateOverdueDocumentNotificationsAsync()
    {
        var overdueDocuments = await _context.Set<Document>()
            .Where(d => d.Status == "PENDING" && 
                       d.DueDate < DateTime.UtcNow && 
                       d.DueDate > DateTime.UtcNow.AddDays(-30))
            .ToListAsync();

        foreach (var document in overdueDocuments)
        {
            var notification = new Notification
            {
                UserId = document.CreatedById,
                Title = "سند تاخیر خورده",
                Message = $"سند '{document.Subject}' باید در تاریخ {document.DueDate:yyyy/MM/dd} تکمیل شود",
                Type = "DOCUMENT_OVERDUE",
                Priority = "HIGH",
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            };

            _context.Set<Notification>().Add(notification);
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// اعلان برای تائیدهای مورد انتظار
    /// </summary>
    public async Task CreatePendingApprovalNotificationsAsync()
    {
        var pendingApprovals = await _context.Set<DocumentAssignment>()
            .Include(da => da.Document)
            .Where(da => da.AssignmentType == "APPROVAL" && 
                        da.Status == "PENDING" &&
                        da.AssignedDate < DateTime.UtcNow.AddDays(-3))
            .ToListAsync();

        foreach (var approval in pendingApprovals)
        {
            var notification = new Notification
            {
                UserId = approval.AssignedToUserId ?? 0,
                Title = "تائید مورد انتظار",
                Message = $"سند '{approval.Document?.Subject}' منتظر تائید شماست",
                Type = "PENDING_APPROVAL",
                Priority = "MEDIUM",
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            };

            _context.Set<Notification>().Add(notification);
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// دریافت اعلان‌های با اولویت بالا
    /// </summary>
    public async Task<List<Notification>> GetHighPriorityNotificationsAsync(int userId, int count = 5)
    {
        return await _context.Set<Notification>()
            .Where(n => n.UserId == userId && 
                       !n.IsRead && 
                       n.Priority == "HIGH" &&
                       (!n.ExpiryDate.HasValue || n.ExpiryDate > DateTime.UtcNow))
            .OrderByDescending(n => n.CreatedDate)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// دریافت اعلان‌های بر اساس نوع
    /// </summary>
    public async Task<List<Notification>> GetNotificationsByTypeAsync(int userId, string type, int count = 10)
    {
        return await _context.Set<Notification>()
            .Where(n => n.UserId == userId && 
                       n.Type == type &&
                       !n.IsRead &&
                       (!n.ExpiryDate.HasValue || n.ExpiryDate > DateTime.UtcNow))
            .OrderByDescending(n => n.CreatedDate)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// پاک‌سازی اعلان‌های منقضی شده
    /// </summary>
    public async Task CleanupExpiredNotificationsAsync()
    {
        var expiredNotifications = await _context.Set<Notification>()
            .Where(n => n.ExpiryDate < DateTime.UtcNow.AddDays(-30)) // قدیمی‌تر از 30 روز
            .ToListAsync();

        _context.Set<Notification>().RemoveRange(expiredNotifications);
        await _context.SaveChangesAsync();
    }
}

/// <summary>
/// درخواست ایجاد اعلان
/// </summary>
public class NotificationRequest
{
    public int UserId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Type { get; set; } = "GENERAL";
    public int? ReferenceId { get; set; }
    public string ReferenceType { get; set; }
    public string Priority { get; set; } = "LOW";
    public DateTime? ExpiryDate { get; set; }
}