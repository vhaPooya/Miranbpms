namespace Automation.Core.Entities;

/// <summary>
/// طبقه‌بندی
/// </summary>
public class Classification : BaseEntity
{
    /// <summary>
    /// کد طبقه‌بندی
    /// </summary>
    public string ClassificationCode { get; set; } = string.Empty;
    
    /// <summary>
    /// نام طبقه‌بندی
    /// </summary>
    public string ClassificationName { get; set; } = string.Empty;
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
}



