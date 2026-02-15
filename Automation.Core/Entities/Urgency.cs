namespace Automation.Core.Entities;

/// <summary>
/// فوریت
/// </summary>
public class Urgency : BaseEntity
{
    /// <summary>
    /// کد فوریت
    /// </summary>
    public string UrgencyCode { get; set; } = string.Empty;
    
    /// <summary>
    /// نام فوریت
    /// </summary>
    public string UrgencyName { get; set; } = string.Empty;
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
}



