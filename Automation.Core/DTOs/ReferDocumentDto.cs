namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ارجاع مدرک
/// </summary>
public class ReferDocumentDto
{
    public int FormId { get; set; }
    public int ActionTypeId { get; set; }
    public List<int>? ReferredToUserIds { get; set; }
    public List<int>? ReferredToRoleIds { get; set; }
    public List<int>? ReferredToDepartmentIds { get; set; }
    public List<int>? ReferredToGroupIds { get; set; }
    public string? Notes { get; set; }
    public DateTime? DueDate { get; set; }
}





