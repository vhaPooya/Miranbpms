namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای به‌روزرسانی نقش
/// </summary>
public class RoleUpdateDto
{
    public string? RoleCode { get; set; }
    public string? RoleName { get; set; }
    public string? Description { get; set; }
    public int? ParentRoleId { get; set; }
    public bool? IsActive { get; set; }
    public List<int>? PermissionIds { get; set; }
}





