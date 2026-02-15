namespace Automation.Core.Entities;

/// <summary>
/// قالب‌های چاپ فرم
/// Print templates for forms
/// </summary>
public class PrintTemplate : BaseEntity
{
    /// <summary>
    /// شناسه فرم
    /// </summary>
    public int FormId { get; set; }
    public virtual Form Form { get; set; } = null!;

    /// <summary>
    /// نام قالب
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// محتوای HTML قالب (با {{FieldName}} placeholders)
    /// </summary>
    public string HtmlContent { get; set; } = string.Empty;

    /// <summary>
    /// استایل‌های CSS
    /// </summary>
    public string? CssStyles { get; set; }

    /// <summary>
    /// اندازه صفحه
    /// </summary>
    public PageSize PageSize { get; set; } = PageSize.A4;

    /// <summary>
    /// جهت صفحه
    /// </summary>
    public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;

    /// <summary>
    /// عرض سفارشی (برای PageSize.Custom)
    /// </summary>
    public string? CustomWidth { get; set; }

    /// <summary>
    /// ارتفاع سفارشی (برای PageSize.Custom)
    /// </summary>
    public string? CustomHeight { get; set; }

    /// <summary>
    /// HTML سربرگ
    /// </summary>
    public string? HeaderHtml { get; set; }

    /// <summary>
    /// HTML پاورقی
    /// </summary>
    public string? FooterHtml { get; set; }

    /// <summary>
    /// حاشیه‌ها (JSON: {top, bottom, left, right})
    /// </summary>
    public string Margins { get; set; } = "{\"top\":\"20mm\",\"bottom\":\"20mm\",\"left\":\"15mm\",\"right\":\"15mm\"}";

    /// <summary>
    /// آیا قالب پیش‌فرض است
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// مسیر فایل Word قالب چاپ
    /// </summary>
    public string? FilePath { get; set; }
}

/// <summary>
/// اندازه صفحه
/// </summary>
public enum PageSize
{
    A4 = 1,
    A5 = 2,
    Letter = 3,
    Legal = 4,
    Custom = 5
}

/// <summary>
/// جهت صفحه
/// </summary>
public enum PageOrientation
{
    /// <summary>
    /// عمودی
    /// </summary>
    Portrait = 1,

    /// <summary>
    /// افقی
    /// </summary>
    Landscape = 2
}


