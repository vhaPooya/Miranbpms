namespace Automation.Core.Entities;

/// <summary>
/// قوانین اعتبارسنجی فرم و فیلدها
/// Form and Field Validation Rules
/// </summary>
public class FormValidation : BaseEntity
{
    /// <summary>
    /// شناسه فرم
    /// </summary>
    public int FormId { get; set; }
    public virtual Form Form { get; set; } = null!;

    /// <summary>
    /// شناسه فیلد (اگر مربوط به فیلد خاص باشد)
    /// </summary>
    public int? FormFieldId { get; set; }
    public virtual FormField? FormField { get; set; }

    /// <summary>
    /// نام قانون اعتبارسنجی
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// نوع اعتبارسنجی
    /// </summary>
    public ValidationType Type { get; set; }

    /// <summary>
    /// مقدار پارامتر (برای min, max, pattern و...)
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// حداقل مقدار
    /// </summary>
    public string? MinValue { get; set; }

    /// <summary>
    /// حداکثر مقدار
    /// </summary>
    public string? MaxValue { get; set; }

    /// <summary>
    /// الگوی regex
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    /// پیام خطای فارسی
    /// </summary>
    public string ErrorMessageFa { get; set; } = string.Empty;

    /// <summary>
    /// پیام خطای انگلیسی
    /// </summary>
    public string? ErrorMessageEn { get; set; }

    /// <summary>
    /// نوع پیام (خطا، هشدار، اطلاع)
    /// </summary>
    public MessageType MessageType { get; set; } = MessageType.Error;

    /// <summary>
    /// اسکریپت سفارشی اعتبارسنجی
    /// </summary>
    public string? CustomScript { get; set; }

    /// <summary>
    /// شرط اعمال قانون
    /// </summary>
    public string? ApplyCondition { get; set; }

    /// <summary>
    /// ترتیب اجرا
    /// </summary>
    public int ExecutionOrder { get; set; } = 0;

    /// <summary>
    /// آیا فعال است
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// آیا در سمت سرور اجرا شود
    /// </summary>
    public bool IsServerSide { get; set; } = true;

    /// <summary>
    /// آیا در سمت کلاینت اجرا شود
    /// </summary>
    public bool IsClientSide { get; set; } = true;
}

/// <summary>
/// نوع اعتبارسنجی
/// </summary>
public enum ValidationType
{
    /// <summary>
    /// اجباری
    /// </summary>
    Required = 1,

    /// <summary>
    /// حداقل طول
    /// </summary>
    MinLength = 2,

    /// <summary>
    /// حداکثر طول
    /// </summary>
    MaxLength = 3,

    /// <summary>
    /// حداقل مقدار
    /// </summary>
    MinValue = 4,

    /// <summary>
    /// حداکثر مقدار
    /// </summary>
    MaxValue = 5,

    /// <summary>
    /// الگوی regex
    /// </summary>
    Pattern = 6,

    /// <summary>
    /// ایمیل
    /// </summary>
    Email = 7,

    /// <summary>
    /// شماره تلفن
    /// </summary>
    Phone = 8,

    /// <summary>
    /// کد ملی
    /// </summary>
    NationalCode = 9,

    /// <summary>
    /// شماره شبا
    /// </summary>
    IBAN = 10,

    /// <summary>
    /// کد پستی
    /// </summary>
    PostalCode = 11,

    /// <summary>
    /// URL
    /// </summary>
    Url = 12,

    /// <summary>
    /// تاریخ شمسی
    /// </summary>
    PersianDate = 13,

    /// <summary>
    /// تاریخ میلادی
    /// </summary>
    GregorianDate = 14,

    /// <summary>
    /// مقایسه با فیلد دیگر
    /// </summary>
    Compare = 15,

    /// <summary>
    /// محدوده تاریخ
    /// </summary>
    DateRange = 16,

    /// <summary>
    /// نوع فایل
    /// </summary>
    FileType = 17,

    /// <summary>
    /// حجم فایل
    /// </summary>
    FileSize = 18,

    /// <summary>
    /// سفارشی
    /// </summary>
    Custom = 99
}

/// <summary>
/// نوع پیام
/// </summary>
public enum MessageType
{
    /// <summary>
    /// خطا
    /// </summary>
    Error = 1,

    /// <summary>
    /// هشدار
    /// </summary>
    Warning = 2,

    /// <summary>
    /// اطلاع‌رسانی
    /// </summary>
    Info = 3,

    /// <summary>
    /// موفقیت
    /// </summary>
    Success = 4
}


