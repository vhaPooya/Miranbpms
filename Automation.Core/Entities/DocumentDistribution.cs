namespace Automation.Core.Entities;

/// <summary>
/// توزیع مدرک بین گیرندگان
/// </summary>
public class DocumentDistribution : BaseEntity
{
    public int DocumentId { get; set; }
    public virtual Document Document { get; set; } = null!;
    public int? RecipientUserId { get; set; }
    public virtual User? RecipientUser { get; set; }
    public int? RecipientDepartmentId { get; set; }
    public virtual Department? RecipientDepartment { get; set; }
    public string DistributionMethod { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING";
    public DateTime DistributedDate { get; set; }
}
