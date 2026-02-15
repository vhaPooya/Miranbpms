namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ایجاد طبقه‌بندی
/// </summary>
public class CreateClassificationDto
{
    public string ClassificationCode { get; set; } = string.Empty;
    public string ClassificationName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
}





