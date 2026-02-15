namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ایجاد واحد
/// </summary>
public class DepartmentCreateDto
{
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? OrganizationId { get; set; }
    public int? ParentDepartmentId { get; set; }
    public bool? IsActive { get; set; }
}





