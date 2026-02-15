namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای به‌روزرسانی نوع مدرک
/// </summary>
public class UpdateDocumentTypeDto
{
    public string DocumentTypeCode { get; set; } = string.Empty;
    public string DocumentTypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
}





