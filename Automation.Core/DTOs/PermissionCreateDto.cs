namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ایجاد مجوز
/// </summary>
public class PermissionCreateDto
{
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
}





