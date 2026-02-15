namespace Automation.Core.Entities;

/// <summary>
/// تم‌های سیستم
/// </summary>
public class Theme : BaseEntity
{
    /// <summary>
    /// نام تم
    /// </summary>
    public string ThemeName { get; set; } = string.Empty;
    
    /// <summary>
    /// رنگ اصلی
    /// </summary>
    public string PrimaryColor { get; set; } = "#007bff";
    
    /// <summary>
    /// رنگ ثانویه
    /// </summary>
    public string SecondaryColor { get; set; } = "#6c757d";
    
    /// <summary>
    /// رنگ پس‌زمینه
    /// </summary>
    public string BackgroundColor { get; set; } = "#ffffff";
    
    /// <summary>
    /// رنگ متن
    /// </summary>
    public string TextColor { get; set; } = "#000000";
    
    /// <summary>
    /// فونت
    /// </summary>
    public string? FontFamily { get; set; }
    
    /// <summary>
    /// CSS اضافی
    /// </summary>
    public string? CustomCss { get; set; }
}



