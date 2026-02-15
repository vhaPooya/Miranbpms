namespace Automation.Core.Entities;

/// <summary>
/// پیام‌های فکس
/// </summary>
public class FaxMessage : BaseEntity
{
    /// <summary>
    /// شناسه مدرک مرتبط (در صورت وجود)
    /// </summary>
    public int? DocumentId { get; set; }
    public virtual Document? Document { get; set; }
    
    /// <summary>
    /// شناسه تنظیمات فکس
    /// </summary>
    public int FaxServiceSettingsId { get; set; }
    public virtual FaxServiceSettings FaxServiceSettings { get; set; } = null!;
    
    /// <summary>
    /// نوع پیام (INCOMING, OUTGOING)
    /// </summary>
    public string MessageType { get; set; } = "OUTGOING";
    
    /// <summary>
    /// شماره فکس فرستنده
    /// </summary>
    public string? FromFaxNumber { get; set; }
    
    /// <summary>
    /// نام فرستنده
    /// </summary>
    public string? FromName { get; set; }
    
    /// <summary>
    /// شماره فکس گیرنده
    /// </summary>
    public string ToFaxNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// نام گیرنده
    /// </summary>
    public string? ToName { get; set; }
    
    /// <summary>
    /// مسیر فایل فکس
    /// </summary>
    public string? FilePath { get; set; }
    
    /// <summary>
    /// تعداد صفحات
    /// </summary>
    public int? PageCount { get; set; }
    
    /// <summary>
    /// وضعیت (SENT, RECEIVED, FAILED, PENDING)
    /// </summary>
    public string Status { get; set; } = "PENDING";
    
    /// <summary>
    /// تاریخ و ساعت ارسال/دریافت
    /// </summary>
    public DateTime? SentReceivedDateTime { get; set; }
    
    /// <summary>
    /// شناسه پیام در Gateway
    /// </summary>
    public string? GatewayMessageId { get; set; }
    
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



