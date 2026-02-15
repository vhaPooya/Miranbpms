namespace Automation.Core.Entities;

/// <summary>
/// فیلدها و کنترل‌های هر فرم
/// Form Fields and Controls
/// </summary>
public class FormField : BaseEntity
{
    /// <summary>
    /// شناسه فرم
    /// </summary>
    public int FormId { get; set; }
    public virtual Form Form { get; set; } = null!;

    /// <summary>
    /// نوع فیلد
    /// </summary>
    public int FieldTypeId { get; set; }
    public virtual FieldType FieldType { get; set; } = null!;

    /// <summary>
    /// فیلد والد (برای ساختار درختی)
    /// </summary>
    public int? ParentFieldId { get; set; }
    public virtual FormField? ParentField { get; set; }

    /// <summary>
    /// فیلدهای فرزند
    /// </summary>
    public virtual ICollection<FormField> ChildFields { get; set; } = new List<FormField>();

    /// <summary>
    /// شناسه یکتا فیلد در فرم
    /// </summary>
    public string FieldKey { get; set; } = string.Empty;

    /// <summary>
    /// نام سیستمی فیلد
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// نام نمایشی فارسی
    /// </summary>
    public string LabelFa { get; set; } = string.Empty;

    /// <summary>
    /// نام نمایشی انگلیسی
    /// </summary>
    public string? LabelEn { get; set; }

    /// <summary>
    /// متن راهنما (Placeholder)
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// توضیحات/راهنمای فیلد
    /// </summary>
    public string? HelpText { get; set; }

    /// <summary>
    /// مقدار پیش‌فرض
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// نام ستون در دیتابیس
    /// </summary>
    public string? DatabaseColumnName { get; set; }

    /// <summary>
    /// نوع داده در دیتابیس
    /// </summary>
    public string? DatabaseColumnType { get; set; }

    /// <summary>
    /// آیا اجباری است
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// آیا فقط خواندنی است
    /// </summary>
    public bool IsReadOnly { get; set; } = false;

    /// <summary>
    /// آیا غیرفعال است
    /// </summary>
    public bool IsDisabled { get; set; } = false;

    /// <summary>
    /// آیا مخفی است
    /// </summary>
    public bool IsHidden { get; set; } = false;

    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// تنظیمات فیلد (JSON)
    /// </summary>
    public string Properties { get; set; } = "{}";

    /// <summary>
    /// استایل‌های فیلد (JSON)
    /// </summary>
    public string Styles { get; set; } = "{}";

    /// <summary>
    /// کلاس‌های CSS
    /// </summary>
    public string? CssClasses { get; set; }

    /// <summary>
    /// استایل‌های inline
    /// </summary>
    public string? InlineStyles { get; set; }

    /// <summary>
    /// رویدادهای JavaScript (JSON)
    /// </summary>
    public string Events { get; set; } = "{}";

    /// <summary>
    /// شرط نمایش (JavaScript)
    /// </summary>
    public string? RenderCondition { get; set; }

    /// <summary>
    /// گزینه‌ها برای select/radio/checkbox (JSON)
    /// </summary>
    public string? Options { get; set; }

    /// <summary>
    /// URL منبع داده
    /// </summary>
    public string? DataSourceUrl { get; set; }

    // ==================== Database Mapping Properties ====================

    /// <summary>
    /// حداکثر طول برای فیلدهای رشته‌ای (nvarchar)
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// آیا فیلد در دیتابیس null‌پذیر است
    /// </summary>
    public bool IsNullable { get; set; } = true;

    /// <summary>
    /// آیا برای این فیلد Index ایجاد شود
    /// </summary>
    public bool HasIndex { get; set; } = false;

    /// <summary>
    /// شناسه تابع پیش‌فرض (برای مقادیر پیش‌فرض پویا)
    /// </summary>
    public int? DefaultFunctionId { get; set; }
    public virtual ScalarFunction? DefaultFunction { get; set; }

    // ==================== Tooltip ====================

    /// <summary>
    /// آیا Tooltip فعال است
    /// </summary>
    public bool TooltipEnabled { get; set; } = false;

    /// <summary>
    /// متن Tooltip
    /// </summary>
    public string? TooltipText { get; set; }

    // ==================== Custom Code ====================

    /// <summary>
    /// CSS سفارشی برای این فیلد (اولویت بالاتر)
    /// </summary>
    public string? CustomCss { get; set; }

    /// <summary>
    /// JavaScript سفارشی برای این فیلد (اولویت بالاتر)
    /// </summary>
    public string? CustomJs { get; set; }

    // ==================== Navigation Properties ====================

    /// <summary>
    /// اعتبارسنجی‌های فیلد
    /// </summary>
    public virtual ICollection<FormValidation> Validations { get; set; } = new List<FormValidation>();

    /// <summary>
    /// دارایی‌های استاتیک فیلد (تصاویر اسلایدر و...)
    /// </summary>
    public virtual ICollection<FormStaticAsset> StaticAssets { get; set; } = new List<FormStaticAsset>();
}


