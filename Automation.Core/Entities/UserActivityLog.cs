namespace Automation.Core.Entities;

/// <summary>
/// لاگ فعالیت کاربر
/// </summary>
public class UserActivityLog : BaseEntity
{
    public int UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
