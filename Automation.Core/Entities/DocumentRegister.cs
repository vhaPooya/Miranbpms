namespace Automation.Core.Entities;

/// <summary>
/// دفتر ثبت نامه‌ها (برای شماره‌گذاری وارده/صادره)
/// </summary>
public class DocumentRegister : BaseEntity
{
    /// <summary>
    /// نوع دفتر (INCOMING, OUTGOING)
    /// </summary>
    public string RegisterType { get; set; } = string.Empty;
    
    /// <summary>
    /// سال
    /// </summary>
    public int Year { get; set; }
    
    /// <summary>
    /// آخرین شماره استفاده شده
    /// </summary>
    public long LastNumber { get; set; } = 0;
    
    /// <summary>
    /// پیشوند شماره (مثل "ص" برای صادره، "و" برای وارده)
    /// </summary>
    public string? Prefix { get; set; }
    
    /// <summary>
    /// شناسه سازمان (اگر برای سازمان خاص باشد)
    /// </summary>
    public int? OrganizationId { get; set; }
    public virtual Organization? Organization { get; set; }
    
    /// <summary>
    /// شناسه بخش (اگر برای بخش خاص باشد)
    /// </summary>
    public int? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }
    
    /// <summary>
    /// فرمت شماره (مثل "{Year}/{Number}" یا "{Prefix}-{Number}")
    /// </summary>
    public string NumberFormat { get; set; } = "{Year}/{Number}";
}



