namespace Automation.Core.Entities;

/// <summary>
/// اختصاص مدرک به کاربر/نقش
/// </summary>
public class DocumentAssignment : BaseEntity
{
    public int DocumentId { get; set; }
    public virtual Document Document { get; set; } = null!;
    public int? AssigneeUserId { get; set; }
    public virtual User? AssigneeUser { get; set; }
    public int? AssigneeRoleId { get; set; }
    public virtual Role? AssigneeRole { get; set; }
    public int? AssignedToUserId { get => AssigneeUserId; set => AssigneeUserId = value; }
    public string AssignmentType { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING";
    public DateTime AssignedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Notes { get; set; }
}
