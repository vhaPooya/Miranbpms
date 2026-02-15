namespace Automation.Core.Entities;

/// <summary>
/// ذخیره ساختار کامل فرم طراحی شده
/// شامل: استایل‌ها، اسکریپت‌ها، چیدمان، کنترل‌ها
/// </summary>
public class FormDesign : BaseEntity
{
    /// <summary>
    /// شناسه فرم
    /// </summary>
    public int FormId { get; set; }
    public virtual Form Form { get; set; } = null!;
    
    /// <summary>
    /// ساختار JSON فرم (DesignData)
    /// شامل تمام کنترل‌ها، چیدمان، استایل‌ها
    /// </summary>
    public string DesignData { get; set; } = string.Empty;
    
    /// <summary>
    /// اسکریپت‌های JavaScript سفارشی فرم
    /// </summary>
    public string? CustomScripts { get; set; }
    
    /// <summary>
    /// استایل‌های CSS سفارشی فرم
    /// </summary>
    public string? CustomStyles { get; set; }
    
    /// <summary>
    /// تنظیمات فرم (JSON)
    /// </summary>
    public string? FormSettings { get; set; }
    
    /// <summary>
    /// نسخه طراحی
    /// </summary>
    public int Version { get; set; } = 1;
    
    /// <summary>
    /// آیا این نسخه فعال است؟
    /// </summary>
    public bool IsActiveVersion { get; set; } = true;
}



