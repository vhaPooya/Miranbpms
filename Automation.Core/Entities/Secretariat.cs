namespace Automation.Core.Entities;

/// <summary>
/// دبیرخانه‌ها
/// </summary>
public class Secretariat : BaseEntity
{
    /// <summary>
    /// کد دبیرخانه
    /// </summary>
    public string SecretariatCode { get; set; } = string.Empty;
    
    /// <summary>
    /// نام دبیرخانه
    /// </summary>
    public string SecretariatName { get; set; } = string.Empty;
    
    /// <summary>
    /// شناسه سازمان
    /// </summary>
    public int OrganizationId { get; set; }
    public virtual Organization Organization { get; set; } = null!;
    
    /// <summary>
    /// آدرس ایمیل دبیرخانه
    /// </summary>
    public string? EmailAddress { get; set; }
    
    /// <summary>
    /// شماره فکس دبیرخانه
    /// </summary>
    public string? FaxNumber { get; set; }
    
    /// <summary>
    /// آیا فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// نوع دفتر (SINGLE = تک دفتری, DOUBLE = دو دفتری)
    /// </summary>
    public string RegistryType { get; set; } = "SINGLE"; // SINGLE, DOUBLE
    
    /// <summary>
    /// قانون شماره‌گذاری وارده (مثل "{Year}/{Number}" یا "{Prefix}-{Year}/{Number}")
    /// </summary>
    public string IncomingNumberFormat { get; set; } = "{Year}/{Number}";
    
    /// <summary>
    /// قانون شماره‌گذاری صادره (مثل "{Year}/{Number}" یا "{Prefix}-{Year}/{Number}")
    /// </summary>
    public string OutgoingNumberFormat { get; set; } = "{Year}/{Number}";
    
    /// <summary>
    /// پیشوند شماره وارده (مثل "و")
    /// </summary>
    public string? IncomingPrefix { get; set; }
    
    /// <summary>
    /// پیشوند شماره صادره (مثل "ص")
    /// </summary>
    public string? OutgoingPrefix { get; set; }
    
    /// <summary>
    /// آیا از سال شمسی استفاده می‌کند؟
    /// </summary>
    public bool UsePersianYear { get; set; } = true;
}



