namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ایجاد فوریت
/// </summary>
public class CreateUrgencyDto
{
    public string UrgencyCode { get; set; } = string.Empty;
    public string UrgencyName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
}





