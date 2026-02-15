using Automation.Core.DTOs;

namespace Automation.Core.Interfaces;

public interface IFormButtonService
{
    /// <summary>لیست انواع دکمه پیشفرض</summary>
    Task<List<FormButtonTypeDto>> GetPredefinedButtonTypesAsync();

    /// <summary>لیست قالب‌های استایل</summary>
    Task<List<ButtonStylePresetDto>> GetStylePresetsAsync();

    /// <summary>دریافت تنظیمات دکمه‌های یک فرم</summary>
    Task<FormButtonConfigDto> GetFormButtonConfigAsync(int formId);

    /// <summary>ذخیره تنظیمات دکمه‌های یک فرم</summary>
    Task<bool> SaveFormButtonConfigAsync(int formId, FormButtonConfigDto config);
}


