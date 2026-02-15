namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای به‌روزرسانی مجوز
/// </summary>
public class PermissionUpdateDto
{
    public string? PermissionCode { get; set; }
    public string? PermissionName { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
}





