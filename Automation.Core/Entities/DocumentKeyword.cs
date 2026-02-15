namespace Automation.Core.Entities;

/// <summary>
/// کلیدواژه‌های مدرک
/// </summary>
public class DocumentKeyword : BaseEntity
{
    /// <summary>
    /// شناسه مدرک
    /// </summary>
    public int DocumentId { get; set; }
    public virtual Document Document { get; set; } = null!;
    
    /// <summary>
    /// کلیدواژه
    /// </summary>
    public string Keyword { get; set; } = string.Empty;
    
    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int DisplayOrder { get; set; }
}



