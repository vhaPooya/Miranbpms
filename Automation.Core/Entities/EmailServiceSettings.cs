namespace Automation.Core.Entities;

/// <summary>
/// تنظیمات سرویس ایمیل
/// </summary>
public class EmailServiceSettings : BaseEntity
{
    /// <summary>
    /// نام تنظیمات
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// آدرس سرور SMTP
    /// </summary>
    public string SmtpServer { get; set; } = string.Empty;
    
    /// <summary>
    /// پورت SMTP
    /// </summary>
    public int SmtpPort { get; set; } = 587;
    
    /// <summary>
    /// نام کاربری
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// رمز عبور (رمزنگاری شده)
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// استفاده از SSL/TLS
    /// </summary>
    public bool UseSsl { get; set; } = true;
    
    /// <summary>
    /// Alias for UseSsl (for compatibility)
    /// </summary>
    public bool EnableSsl { get => UseSsl; set => UseSsl = value; }
    
    /// <summary>
    /// آدرس ایمیل فرستنده پیش‌فرض
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Alias for FromEmail (for compatibility)
    /// </summary>
    public string FromAddress { get => FromEmail; set => FromEmail = value; }
    
    /// <summary>
    /// نام فرستنده پیش‌فرض
    /// </summary>
    public string FromName { get; set; } = string.Empty;
    
    /// <summary>
    /// آدرس سرور IMAP (برای دریافت)
    /// </summary>
    public string? ImapServer { get; set; }
    
    /// <summary>
    /// پورت IMAP
    /// </summary>
    public int? ImapPort { get; set; } = 993;
    
    /// <summary>
    /// آدرس سرور POP3 (برای دریافت)
    /// </summary>
    public string? Pop3Server { get; set; }
    
    /// <summary>
    /// پورت POP3
    /// </summary>
    public int? Pop3Port { get; set; } = 995;
    
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



