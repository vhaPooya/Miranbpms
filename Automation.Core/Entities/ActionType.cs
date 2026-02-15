namespace Automation.Core.Entities;

/// <summary>
/// انواع اکشن‌ها (عملگرها)
/// </summary>
public class ActionType : BaseEntity
{
    /// <summary>
    /// کد اکشن (مثل ACTION_FOR_SIGNATURE)
    /// </summary>
    public string ActionCode { get; set; } = string.Empty;
    
    /// <summary>
    /// نام فارسی اکشن
    /// </summary>
    public string ActionNameFa { get; set; } = string.Empty;
    
    /// <summary>
    /// نام انگلیسی اکشن
    /// </summary>
    public string ActionNameEn { get; set; } = string.Empty;
    
    /// <summary>
    /// آیا نامه در این حالت قابل ویرایش است؟ (true = باز, false = قفل)
    /// </summary>
    public bool IsEditable { get; set; } = false;
    
    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// ارجاع‌های با این Action Type
    /// </summary>
    public virtual ICollection<DocumentReferral> DocumentReferrals { get; set; } = new List<DocumentReferral>();
}



