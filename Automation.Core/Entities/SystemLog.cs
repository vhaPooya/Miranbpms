namespace Automation.Core.Entities;

/// <summary>
/// لاگ سیستمی
/// </summary>
public class SystemLog : BaseEntity
{
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Source { get; set; }
    public string? Exception { get; set; }
}
