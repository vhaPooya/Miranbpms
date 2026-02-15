namespace Automation.Core.Entities;

/// <summary>
/// اختصاص وظیفه به کاربر
/// </summary>
public class TaskAssignment : BaseEntity
{
    public int WorkflowInstanceId { get; set; }
    public int WorkflowTokenId { get; set; }
    public virtual WorkflowToken? WorkflowToken { get; set; }
    public int NodeId { get; set; }
    public int AssigneeId { get; set; }
    public string Status { get; set; } = "PENDING";
    public string? AssignmentType { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Comment { get; set; }
}
