namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای به‌روزرسانی واحد
/// </summary>
public class DepartmentUpdateDto
{
    public string? DepartmentCode { get; set; }
    public string? DepartmentName { get; set; }
    public string? Description { get; set; }
    public int? OrganizationId { get; set; }
    public int? ParentDepartmentId { get; set; }
    public bool? IsActive { get; set; }
}





