namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ایجاد گروه
/// </summary>
public class CreateGroupDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}





