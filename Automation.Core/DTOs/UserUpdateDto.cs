namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای به‌روزرسانی کاربر
/// </summary>
public class UserUpdateDto
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public int? DepartmentId { get; set; }
    public int? OrganizationId { get; set; }
    public bool? IsActive { get; set; }
    public List<int>? RoleIds { get; set; }
}





