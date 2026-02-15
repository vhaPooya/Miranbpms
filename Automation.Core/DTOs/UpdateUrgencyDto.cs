namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای به‌روزرسانی فوریت
/// </summary>
public class UpdateUrgencyDto
{
    public string UrgencyCode { get; set; } = string.Empty;
    public string UrgencyName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
}





