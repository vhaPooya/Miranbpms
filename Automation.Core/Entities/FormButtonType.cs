namespace Automation.Core.Entities;

/// <summary>
/// انواع دکمه‌های پیش‌فرض عملیاتی فرم (18 نوع)
/// </summary>
public class FormButtonType : BaseEntity
{
    /// <summary>کد یکتای دکمه مثل REFERRAL, PRINT, ATTACHMENT</summary>
    public string ButtonCode { get; set; } = string.Empty;

    public string NameFa { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;

    /// <summary>آیکون پیشفرض (کلاس CSS مثل bi-share)</summary>
    public string DefaultIcon { get; set; } = string.Empty;

    /// <summary>رنگ پیشفرض (کلاس Bootstrap مثل btn-warning)</summary>
    public string DefaultColor { get; set; } = string.Empty;

    /// <summary>دسته‌بندی: document, process, utility</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>نام تابع JavaScript هندلر</summary>
    public string ActionHandler { get; set; } = string.Empty;

    /// <summary>آیا مدال باز میکند</summary>
    public bool OpensModal { get; set; }

    /// <summary>شناسه مدال (مثل referModal, attachmentModal)</summary>
    public string? ModalId { get; set; }

    /// <summary>ترتیب نمایش در UI تنظیمات</summary>
    public int DisplayOrder { get; set; }

    /// <summary>توضیحات</summary>
    public string? Description { get; set; }
}


