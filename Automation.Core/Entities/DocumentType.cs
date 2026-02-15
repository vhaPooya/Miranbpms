namespace Automation.Core.Entities;

/// <summary>
/// نوع مدرک
/// </summary>
public class DocumentType : BaseEntity
{
    /// <summary>
    /// کد نوع مدرک
    /// </summary>
    public string DocumentTypeCode { get; set; } = string.Empty;
    
    /// <summary>
    /// نام نوع مدرک
    /// </summary>
    public string DocumentTypeName { get; set; } = string.Empty;
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
}



