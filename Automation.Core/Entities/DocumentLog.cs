namespace Automation.Core.Entities;

/// <summary>
/// لاگ فعالیت سند
/// </summary>
public class DocumentLog : BaseEntity
{
    public int DocumentId { get; set; }
    public int UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
}
