namespace Automation.Core.Entities;

/// <summary>
/// لاگ فعالیت گردش کار
/// </summary>
public class WorkflowLog : BaseEntity
{
    public int WorkflowInstanceId { get; set; }
    public string LogLevel { get; set; } = "INFO";
    public string? ActivityType { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
