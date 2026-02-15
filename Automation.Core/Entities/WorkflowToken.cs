namespace Automation.Core.Entities;

/// <summary>
/// توکن گردش کار (وضعیت فعلی در یک نود)
/// </summary>
public class WorkflowToken : BaseEntity
{
    public int WorkflowInstanceId { get; set; }
    public virtual WorkflowInstance WorkflowInstance { get; set; } = null!;
    public int NodeId { get; set; }
    public virtual WorkflowNode Node { get; set; } = null!;
    public string Status { get; set; } = "ACTIVE";
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Metadata { get; set; }
    public string? ErrorMessage { get; set; }
}
