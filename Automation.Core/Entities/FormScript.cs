namespace Automation.Core.Entities;

/// <summary>
/// اسکریپت‌های JavaScript فرم
/// Form Scripts (JavaScript)
/// </summary>
public class FormScript : BaseEntity
{
    /// <summary>
    /// شناسه فرم
    /// </summary>
    public int FormId { get; set; }
    public virtual Form Form { get; set; } = null!;

    /// <summary>
    /// نام اسکریپت
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// نوع اسکریپت
    /// </summary>
    public ScriptType Type { get; set; } = ScriptType.Custom;

    /// <summary>
    /// رویداد مرتبط
    /// </summary>
    public ScriptEvent Event { get; set; } = ScriptEvent.Custom;

    /// <summary>
    /// محتوای اسکریپت
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// ترتیب اجرا
    /// </summary>
    public int ExecutionOrder { get; set; } = 0;

    /// <summary>
    /// آیا فعال است
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// نوع اسکریپت
/// </summary>
public enum ScriptType
{
    /// <summary>
    /// اسکریپت سفارشی
    /// </summary>
    Custom = 1,

    /// <summary>
    /// اعتبارسنجی
    /// </summary>
    Validation = 2,

    /// <summary>
    /// محاسبه
    /// </summary>
    Calculation = 3,

    /// <summary>
    /// فراخوانی API
    /// </summary>
    ApiCall = 4,

    /// <summary>
    /// شرط نمایش
    /// </summary>
    Conditional = 5
}

/// <summary>
/// رویداد اسکریپت
/// </summary>
public enum ScriptEvent
{
    Custom = 0,
    OnLoad = 1,
    OnSubmit = 2,
    BeforeSubmit = 3,
    AfterSubmit = 4,
    OnChange = 5,
    OnFocus = 6,
    OnBlur = 7,
    OnClick = 8,
    OnValidate = 9,
    OnReset = 10
}


