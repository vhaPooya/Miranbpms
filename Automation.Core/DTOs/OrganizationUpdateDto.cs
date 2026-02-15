namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای به‌روزرسانی سازمان
/// </summary>
public class OrganizationUpdateDto
{
    public string? OrganizationCode { get; set; }
    public string? OrganizationName { get; set; }
    public bool? IsParentOrganization { get; set; }
    public bool? IsSubsidiary { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}





