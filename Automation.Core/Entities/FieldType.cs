namespace Automation.Core.Entities;

/// <summary>
/// انواع فیلدها و کنترل‌های فرم
/// Field Types for form controls
/// </summary>
public class FieldType : BaseEntity
{
    /// <summary>
    /// نام سیستمی نوع فیلد
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// نام نمایشی فارسی
    /// </summary>
    public string DisplayNameFa { get; set; } = string.Empty;

    /// <summary>
    /// نام نمایشی انگلیسی
    /// </summary>
    public string DisplayNameEn { get; set; } = string.Empty;

    /// <summary>
    /// دسته‌بندی نوع فیلد (Input, Layout, Static, Action)
    /// </summary>
    public FieldTypeCategory Category { get; set; }

    /// <summary>
    /// آیکون فیلد
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// تنظیمات پیش‌فرض (JSON)
    /// </summary>
    public string DefaultProperties { get; set; } = "{}";

    /// <summary>
    /// استایل‌های پیش‌فرض (JSON)
    /// </summary>
    public string DefaultStyles { get; set; } = "{}";

    /// <summary>
    /// قالب HTML برای رندر
    /// </summary>
    public string HtmlTemplate { get; set; } = string.Empty;

    /// <summary>
    /// قالب Razor برای تولید کد
    /// </summary>
    public string RazorTemplate { get; set; } = string.Empty;

    /// <summary>
    /// آیا کانتینر است (می‌تواند فرزند داشته باشد)
    /// </summary>
    public bool IsContainer { get; set; } = false;

    /// <summary>
    /// آیا قابل ایجاد ستون در دیتابیس است
    /// </summary>
    public bool CanCreateColumn { get; set; } = true;

    /// <summary>
    /// ترتیب نمایش در پنل کنترل‌ها
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// فیلدهای این نوع
    /// </summary>
    public virtual ICollection<FormField> FormFields { get; set; } = new List<FormField>();
}

/// <summary>
/// دسته‌بندی انواع فیلدها
/// </summary>
public enum FieldTypeCategory
{
    /// <summary>
    /// کنترل‌های ورودی
    /// </summary>
    Input = 1,

    /// <summary>
    /// کنترل‌های چیدمان
    /// </summary>
    Layout = 2,

    /// <summary>
    /// عناصر ایستا
    /// </summary>
    Static = 3,

    /// <summary>
    /// دکمه‌ها و اقدامات
    /// </summary>
    Action = 4
}


