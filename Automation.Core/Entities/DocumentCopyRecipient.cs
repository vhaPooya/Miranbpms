namespace Automation.Core.Entities;

/// <summary>
/// گیرندگان رونوشت (Copies)
/// </summary>
public class DocumentCopyRecipient : BaseEntity
{
    /// <summary>
    /// شناسه مدرک
    /// </summary>
    public int DocumentId { get; set; }
    public virtual Document Document { get; set; } = null!;
    
    /// <summary>
    /// نام گیرنده
    /// </summary>
    public string RecipientName { get; set; } = string.Empty;
    
    /// <summary>
    /// نوع اقدام (جهت اطلاع، جهت اقدام، جهت پیگیری)
    /// </summary>
    public string ActionType { get; set; } = "FOR_INFORMATION"; // FOR_INFORMATION, FOR_ACTION, FOR_FOLLOWUP
    
    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int DisplayOrder { get; set; }
}



