namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای به‌روزرسانی طبقه‌بندی
/// </summary>
public class UpdateClassificationDto
{
    public string ClassificationCode { get; set; } = string.Empty;
    public string ClassificationName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
}





