namespace Automation.Core.Entities;

/// <summary>
/// تنظیمات دکمه عملیاتی تعریف‌شده برای یک فرم خاص
/// </summary>
public class FormButton : BaseEntity
{
    // ===== روابط اصلی =====

    /// <summary>فرم مربوطه</summary>
    public int FormId { get; set; }
    public virtual Form Form { get; set; } = null!;

    /// <summary>نوع دکمه پیشفرض (null برای دکمه سرویس فرآیندی سفارشی)</summary>
    public int? FormButtonTypeId { get; set; }
    public virtual FormButtonType? FormButtonType { get; set; }

    // ===== نوع دکمه =====

    /// <summary>آیا دکمه اجرای سرویس فرآیندی است</summary>
    public bool IsProcessServiceButton { get; set; }

    // ===== نمایش =====

    /// <summary>عنوان سفارشی فارسی (null = استفاده از پیشفرض FormButtonType)</summary>
    public string? CustomTitleFa { get; set; }
    public string? CustomTitleEn { get; set; }

    /// <summary>ترتیب نمایش در نوار دکمه‌ها</summary>
    public int DisplayOrder { get; set; }

    /// <summary>حالت نمایش: icon_only, title_only, icon_and_title</summary>
    public string DisplayMode { get; set; } = "icon_and_title";

    /// <summary>آیکون سفارشی (null = استفاده از پیشفرض)</summary>
    public string? CustomIcon { get; set; }

    // ===== استایل =====

    /// <summary>تنظیمات استایل JSON: { color, bgColor, fontFamily, fontSize, width, height, padding, margin, borderRadius }</summary>
    public string StyleSettings { get; set; } = "{}";

    /// <summary>قالب استایل آماده (null = استایل سفارشی)</summary>
    public int? ButtonStylePresetId { get; set; }
    public virtual ButtonStylePreset? ButtonStylePreset { get; set; }

    // ===== سرویس فرآیندی =====

    /// <summary>شناسه سرویس/فرآیند برای اجرا (فعلا غیرفعال)</summary>
    public int? WorkflowServiceId { get; set; }

    /// <summary>وارث خاصیت کدام دکمه پیشفرض</summary>
    public int? InheritFromButtonTypeId { get; set; }
    public virtual FormButtonType? InheritFromButtonType { get; set; }

    /// <summary>زمان اجرای سرویس نسبت به دکمه وارث: before, after</summary>
    public string? ExecutionTiming { get; set; }

    /// <summary>تنظیمات سرویس فرآیندی JSON (پارامترها، شرایط)</summary>
    public string? ProcessServiceConfig { get; set; }

    // ===== سایر =====

    /// <summary>شرط نمایش (JavaScript expression)</summary>
    public string? VisibilityCondition { get; set; }

    /// <summary>کد مجوز موردنیاز</summary>
    public string? RequiredPermission { get; set; }

    /// <summary>پیام تایید قبل از اجرا</summary>
    public string? ConfirmationMessage { get; set; }

    /// <summary>فعال/غیرفعال</summary>
    public bool IsEnabled { get; set; } = true;
}


