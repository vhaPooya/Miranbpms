namespace Automation.Core.Entities;

/// <summary>
/// دارایی‌های استاتیک فرم (تصاویر اسلایدر، داده‌های ثابت جداول و...)
/// Static assets for forms like slider images, static table data
/// </summary>
public class FormStaticAsset : BaseEntity
{
    /// <summary>
    /// شناسه فرم
    /// </summary>
    public int FormId { get; set; }
    public virtual Form Form { get; set; } = null!;

    /// <summary>
    /// شناسه فیلد (اختیاری - برای اسلایدر، جدول و...)
    /// </summary>
    public int? FormFieldId { get; set; }
    public virtual FormField? FormField { get; set; }

    /// <summary>
    /// نوع دارایی
    /// </summary>
    public AssetType AssetType { get; set; }

    /// <summary>
    /// نام فایل اصلی
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// مسیر فایل رمزنگاری شده
    /// </summary>
    public string EncryptedPath { get; set; } = string.Empty;

    /// <summary>
    /// نوع MIME
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// حجم فایل به بایت
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// متادیتای اضافی (JSON)
    /// برای تصویر: {caption, altText, linkUrl, animationType}
    /// برای داده ثابت: {columns: [...], rows: [...]}
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// عنوان/کپشن
    /// </summary>
    public string? Caption { get; set; }

    /// <summary>
    /// نوع انیمیشن (برای اسلایدر)
    /// </summary>
    public SliderAnimationType? AnimationType { get; set; }
}

/// <summary>
/// انواع دارایی
/// </summary>
public enum AssetType
{
    /// <summary>
    /// تصویر
    /// </summary>
    Image = 1,

    /// <summary>
    /// داده ثابت (برای جدول)
    /// </summary>
    StaticData = 2,

    /// <summary>
    /// سند/فایل
    /// </summary>
    Document = 3,

    /// <summary>
    /// ویدیو
    /// </summary>
    Video = 4
}

/// <summary>
/// انواع انیمیشن اسلایدر
/// </summary>
public enum SliderAnimationType
{
    /// <summary>
    /// محو شدن
    /// </summary>
    FadeIn = 1,

    /// <summary>
    /// اسلاید از چپ
    /// </summary>
    SlideLeft = 2,

    /// <summary>
    /// بزرگنمایی
    /// </summary>
    ZoomIn = 3
}


