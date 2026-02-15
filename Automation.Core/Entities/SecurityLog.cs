namespace Automation.Core.Entities;

/// <summary>
/// لاگ رویدادهای امنیتی
/// </summary>
public class SecurityLog : BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Resource { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
}
