namespace Automation.Core.Entities;

/// <summary>
/// لاگ تغییرات موجودیت‌ها
/// </summary>
public class AuditLog : BaseEntity
{
    public int? UserId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
