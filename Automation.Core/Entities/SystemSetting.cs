namespace Automation.Core.Entities;

/// <summary>
/// تنظیمات سیستم
/// </summary>
public class SystemSetting : BaseEntity
{
    /// <summary>
    /// کلید تنظیمات
    /// </summary>
    public string SettingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// مقدار تنظیمات (JSON یا String)
    /// </summary>
    public string SettingValue { get; set; } = string.Empty;
    
    /// <summary>
    /// نوع تنظیمات
    /// </summary>
    public string SettingType { get; set; } = "String"; // String, JSON, Number, Boolean
    
    /// <summary>
    /// دسته‌بندی تنظیمات
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
}



