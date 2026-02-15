namespace Automation.Core.Entities;

/// <summary>
/// پیام‌های ایمیل
/// </summary>
public class EmailMessage : BaseEntity
{
    /// <summary>
    /// شناسه مدرک مرتبط (در صورت وجود)
    /// </summary>
    public int? DocumentId { get; set; }
    public virtual Document? Document { get; set; }
    
    /// <summary>
    /// شناسه تنظیمات ایمیل
    /// </summary>
    public int EmailServiceSettingsId { get; set; }
    public virtual EmailServiceSettings EmailServiceSettings { get; set; } = null!;
    
    /// <summary>
    /// نوع پیام (INCOMING, OUTGOING)
    /// </summary>
    public string MessageType { get; set; } = "OUTGOING";
    
    /// <summary>
    /// آدرس ایمیل فرستنده
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// نام فرستنده
    /// </summary>
    public string? FromName { get; set; }
    
    /// <summary>
    /// آدرس ایمیل گیرنده
    /// </summary>
    public string ToEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// نام گیرنده
    /// </summary>
    public string? ToName { get; set; }
    
    /// <summary>
    /// CC
    /// </summary>
    public string? Cc { get; set; }
    
    /// <summary>
    /// BCC
    /// </summary>
    public string? Bcc { get; set; }
    
    /// <summary>
    /// موضوع
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// محتوا
    /// </summary>
    public string Body { get; set; } = string.Empty;
    
    /// <summary>
    /// آیا HTML است؟
    /// </summary>
    public bool IsHtml { get; set; } = false;
    
    /// <summary>
    /// وضعیت (SENT, RECEIVED, DRAFT, FAILED)
    /// </summary>
    public string Status { get; set; } = "DRAFT";
    
    /// <summary>
    /// تاریخ و ساعت ارسال/دریافت
    /// </summary>
    public DateTime? SentReceivedDateTime { get; set; }
    
    /// <summary>
    /// شناسه پیام در سرور ایمیل
    /// </summary>
    public string? ServerMessageId { get; set; }
    
    /// <summary>
    /// پیام خطا (در صورت وجود)
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// شناسه کاربر ایجادکننده
    /// </summary>
    public int? CreatedByUserId { get; set; }
    public virtual User? CreatedByUser { get; set; }
}



