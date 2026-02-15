namespace Automation.Core.DTOs;

/// <summary>
/// DTO کامل تنظیمات دکمه‌های یک فرم (برای ذخیره یکجا)
/// </summary>
public class FormButtonConfigDto
{
    /// <summary>تنظیمات نوار دکمه‌ها (JSON)</summary>
    public string? ButtonBarSettings { get; set; }

    /// <summary>لیست دکمه‌های فرم</summary>
    public List<FormButtonDto> Buttons { get; set; } = new();
}



