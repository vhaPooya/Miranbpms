namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ایجاد نوع مدرک
/// </summary>
public class CreateDocumentTypeDto
{
    public string DocumentTypeCode { get; set; } = string.Empty;
    public string DocumentTypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
}





