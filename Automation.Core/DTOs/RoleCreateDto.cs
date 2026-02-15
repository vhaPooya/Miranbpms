namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ایجاد نقش
/// </summary>
public class RoleCreateDto
{
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentRoleId { get; set; }
    public bool? IsActive { get; set; }
    public List<int>? PermissionIds { get; set; }
}





