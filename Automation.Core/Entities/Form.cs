using System.ComponentModel.DataAnnotations;

namespace Automation.Core.Entities;

/// <summary>
/// فرم‌های طراحی شده در فرم‌ساز
/// Forms designed in Form Builder
/// </summary>
public class Form : BaseEntity
{
    /// <summary>
    /// کد شناسه فرم (کلید بیزینس - 3 رقمی)
    /// </summary>
    [Range(100, 999, ErrorMessage = "کد فرم باید 3 رقمی باشد")]
    public int Code { get; set; }

    /// <summary>
    /// کد نمایشی فرم (مثل FRM-001)
    /// </summary>
    public string FormCode { get; set; } = string.Empty;

    /// <summary>
    /// نام فارسی فرم
    /// </summary>
    public string NameFa { get; set; } = string.Empty;

    /// <summary>
    /// نام انگلیسی فرم
    /// </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات فرم
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// دسته‌بندی فرم
    /// </summary>
    public int? CategoryId { get; set; }
    public virtual FormCategory? Category { get; set; }

    /// <summary>
    /// نسخه فرم
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// نوع چیدمان اصلی (Flex, Grid, Table)
    /// </summary>
    public FormLayoutType LayoutType { get; set; } = FormLayoutType.Flex;

    /// <summary>
    /// قانون شماره‌گذاری فرم
    /// </summary>
    public string? NumberingRule { get; set; }

    /// <summary>
    /// پیشوند شماره‌گذاری
    /// </summary>
    public string? NumberingPrefix { get; set; }

    /// <summary>
    /// آخرین شماره استفاده شده
    /// </summary>
    public long LastNumber { get; set; } = 0;

    /// <summary>
    /// نوع فرم (اداری، تقویم، مالی، بایگانی)
    /// </summary>
    public FormCategoryType FormType { get; set; } = FormCategoryType.Administrative;

    /// <summary>
    /// تنظیمات پس‌زمینه (رنگ یا تصویر) - JSON
    /// </summary>
    public string BackgroundSettings { get; set; } = "{}";

    /// <summary>
    /// اندازه صحنه طراحی (JSON: Width, Height)
    /// </summary>
    public string CanvasSize { get; set; } = "{\"width\":\"100%\",\"height\":\"auto\"}";

    /// <summary>
    /// نام جدول دیتابیس
    /// </summary>
    public string? DatabaseTableName { get; set; }

    /// <summary>
    /// آیا جدول دیتابیس ایجاد شده
    /// </summary>
    public bool IsTableCreated { get; set; } = false;

    /// <summary>
    /// ساختار کامل فرم بصورت XML (Metadata)
    /// </summary>
    public string? StructureXml { get; set; }

    /// <summary>
    /// استایل‌های سفارشی CSS
    /// </summary>
    public string? CustomStyles { get; set; }

    /// <summary>
    /// اسکریپت‌های سفارشی JavaScript
    /// </summary>
    public string? CustomScripts { get; set; }

    /// <summary>
    /// تنظیمات Bootstrap (JSON)
    /// </summary>
    public string BootstrapSettings { get; set; } = "{}";

    /// <summary>
    /// تنظیمات فرم (JSON)
    /// </summary>
    public string FormSettings { get; set; } = "{}";

    /// <summary>
    /// داده‌های طراحی فرم (JSON کامل)
    /// </summary>
    public string? DesignData { get; set; }

    /// <summary>
    /// آیا منتشر شده
    /// </summary>
    public bool IsPublished { get; set; } = false;

    /// <summary>
    /// تاریخ انتشار
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// فیلدهای فرم
    /// </summary>
    public virtual ICollection<FormField> Fields { get; set; } = new List<FormField>();

    /// <summary>
    /// اسکریپت‌های فرم
    /// </summary>
    public virtual ICollection<FormScript> Scripts { get; set; } = new List<FormScript>();

    /// <summary>
    /// استایل‌های فرم
    /// </summary>
    public virtual ICollection<FormStyle> Styles { get; set; } = new List<FormStyle>();

    /// <summary>
    /// قوانین اعتبارسنجی فرم
    /// </summary>
    public virtual ICollection<FormValidation> Validations { get; set; } = new List<FormValidation>();

    /// <summary>
    /// تنظیمات نوار دکمه‌ها JSON: { layout, alignment, gap, presetId }
    /// </summary>
    public string? ButtonBarSettings { get; set; }

    /// <summary>
    /// دکمه‌های عملیاتی فرم
    /// </summary>
    public virtual ICollection<FormButton> Buttons { get; set; } = new List<FormButton>();
}

/// <summary>
/// نوع دسته‌بندی فرم
/// </summary>
public enum FormCategoryType
{
    /// <summary>
    /// نامه اداری
    /// </summary>
    Administrative = 1,

    /// <summary>
    /// وابسته به تقویم (جلسات)
    /// </summary>
    Scheduled = 2,

    /// <summary>
    /// مالی
    /// </summary>
    Financial = 3,

    /// <summary>
    /// بایگانی
    /// </summary>
    Archive = 4,
    
    /// <summary>
    /// استخدامی
    /// </summary>
    Recruitment = 5,
    
    /// <summary>
    /// عمومی
    /// </summary>
    General = 6
}

/// <summary>
/// نوع چیدمان فرم
/// </summary>
public enum FormLayoutType
{
    /// <summary>
    /// چیدمان Flexbox
    /// </summary>
    Flex = 1,

    /// <summary>
    /// چیدمان Grid
    /// </summary>
    Grid = 2,

    /// <summary>
    /// چیدمان جدولی
    /// </summary>
    Table = 3
}


