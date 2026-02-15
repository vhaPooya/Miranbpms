namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ایجاد سازمان
/// </summary>
public class OrganizationCreateDto
{
    public string OrganizationCode { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public bool IsParentOrganization { get; set; } = false;
    public bool IsSubsidiary { get; set; } = false;
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}





