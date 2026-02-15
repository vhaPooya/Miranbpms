namespace Automation.Core.Entities;

/// <summary>
/// نحوه دریافت
/// </summary>
public class ReceiptMethod : BaseEntity
{
    /// <summary>
    /// کد نحوه دریافت
    /// </summary>
    public string ReceiptMethodCode { get; set; } = string.Empty;
    
    /// <summary>
    /// نام نحوه دریافت
    /// </summary>
    public string ReceiptMethodName { get; set; } = string.Empty;
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
}



