namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ایجاد کاربر
/// </summary>
public class UserCreateDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public int? DepartmentId { get; set; }
    public int? OrganizationId { get; set; }
    public bool? IsActive { get; set; }
    public List<int>? RoleIds { get; set; }
}





