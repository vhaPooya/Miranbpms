namespace Automation.Core.Entities;

/// <summary>
/// تنظیمات سرویس فکس
/// </summary>
public class FaxServiceSettings : BaseEntity
{
    /// <summary>
    /// نام تنظیمات
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// نوع Gateway (T38, SIP, EMAIL_TO_FAX, API)
    /// </summary>
    public string GatewayType { get; set; } = "API";
    
    /// <summary>
    /// آدرس Gateway
    /// </summary>
    public string? GatewayUrl { get; set; }
    
    /// <summary>
    /// API Key
    /// </summary>
    public string? ApiKey { get; set; }
    
    /// <summary>
    /// نام کاربری
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// رمز عبور
    /// </summary>
    public string? Password { get; set; }
    
    /// <summary>
    /// شماره فکس پیش‌فرض
    /// </summary>
    public string? DefaultFaxNumber { get; set; }
    
    /// <summary>
    /// Alias for DefaultFaxNumber (for compatibility)
    /// </summary>
    public string? SenderFaxNumber { get => DefaultFaxNumber; set => DefaultFaxNumber = value; }
    
    /// <summary>
    /// شناسه دبیرخانه
    /// </summary>
    public int? SecretariatId { get; set; }
    public virtual Secretariat? Secretariat { get; set; }
    
    /// <summary>
    /// آیا فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// آیا تنظیمات پیش‌فرض است؟
    /// </summary>
    public bool IsDefault { get; set; } = false;
}



