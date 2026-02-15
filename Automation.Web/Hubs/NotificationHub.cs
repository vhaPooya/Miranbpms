using Microsoft.AspNetCore.SignalR;
using Automation.Infrastructure.Services.Notifications;
using Automation.Core.Entities;

namespace Automation.Web.Hubs;

/// <summary>
/// هاب اعلان‌های بلادرنگ
/// </summary>
public class NotificationHub : Hub
{
    private readonly NotificationService _notificationService;

    public NotificationHub(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// اتصال کاربر
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// قطع اتصال کاربر
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// ارسال اعلان به کاربر خاص
    /// </summary>
    public async Task SendNotificationToUser(int userId, Notification notification)
    {
        await Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);
    }

    /// <summary>
    /// ارسال اعلان به گروه
    /// </summary>
    public async Task SendNotificationToGroup(string groupName, Notification notification)
    {
        await Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
    }

    /// <summary>
    /// ارسال پیام سیستمی
    /// </summary>
    public async Task SendSystemMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveSystemMessage", message);
    }

    /// <summary>
    /// به‌روزرسانی تعداد اعلان‌های خوانده نشده
    /// </summary>
    public async Task UpdateUnreadCount(int userId)
    {
        var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
        await Clients.User(userId.ToString()).SendAsync("UpdateUnreadCount", count);
    }

    /// <summary>
    /// ارسال وضعیت سیستم
    /// </summary>
    public async Task SendSystemStatus(string status)
    {
        await Clients.All.SendAsync("ReceiveSystemStatus", status);
    }

    /// <summary>
    /// مدیریت اتصال
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("UserId")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            
            // ارسال تعداد اعلان‌های خوانده نشده
            var unreadCount = await _notificationService.GetUnreadNotificationCountAsync(int.Parse(userId));
            await Clients.Caller.SendAsync("UpdateUnreadCount", unreadCount);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// مدیریت قطع اتصال
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.User?.FindFirst("UserId")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}