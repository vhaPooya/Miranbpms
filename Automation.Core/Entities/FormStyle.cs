namespace Automation.Core.Entities;

/// <summary>
/// استایل‌های CSS فرم
/// Form Styles (CSS)
/// </summary>
public class FormStyle : BaseEntity
{
    /// <summary>
    /// شناسه فرم
    /// </summary>
    public int FormId { get; set; }
    public virtual Form Form { get; set; } = null!;

    /// <summary>
    /// نام استایل
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// نوع استایل
    /// </summary>
    public StyleType Type { get; set; } = StyleType.Custom;

    /// <summary>
    /// سلکتور CSS
    /// </summary>
    public string? Selector { get; set; }

    /// <summary>
    /// محتوای CSS
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// اولویت اعمال
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// آیا فعال است
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// آیا برای تم خاصی است
    /// </summary>
    public string? ThemeName { get; set; }

    /// <summary>
    /// Media Query
    /// </summary>
    public string? MediaQuery { get; set; }
}

/// <summary>
/// نوع استایل
/// </summary>
public enum StyleType
{
    /// <summary>
    /// استایل سفارشی
    /// </summary>
    Custom = 1,

    /// <summary>
    /// استایل Bootstrap
    /// </summary>
    Bootstrap = 2,

    /// <summary>
    /// متغیرهای CSS
    /// </summary>
    Variables = 3,

    /// <summary>
    /// استایل تم
    /// </summary>
    Theme = 4,

    /// <summary>
    /// استایل چاپ
    /// </summary>
    Print = 5,

    /// <summary>
    /// استایل موبایل
    /// </summary>
    Mobile = 6
}


