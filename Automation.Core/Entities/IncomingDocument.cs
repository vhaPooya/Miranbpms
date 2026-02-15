namespace Automation.Core.Entities;

/// <summary>
/// نامه‌های وارده
/// </summary>
public class IncomingDocument : BaseEntity
{
    /// <summary>
    /// شناسه مدرک پایه
    /// </summary>
    public int DocumentId { get; set; }
    public virtual Document Document { get; set; } = null!;
    
    /// <summary>
    /// شماره وارده (از دفتر)
    /// </summary>
    public string IncomingNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// تاریخ وارده
    /// </summary>
    public DateTime IncomingDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// فرستنده (نام شخص/سازمان)
    /// </summary>
    public string SenderName { get; set; } = string.Empty;
    
    /// <summary>
    /// شماره نامه فرستنده
    /// </summary>
    public string? SenderNumber { get; set; }
    
    /// <summary>
    /// تاریخ نامه فرستنده
    /// </summary>
    public DateTime? SenderDate { get; set; }
    
    /// <summary>
    /// روش دریافت (SCAN, EMAIL, MANUAL, SYSTEM)
    /// </summary>
    public string ReceiptMethod { get; set; } = "MANUAL";
    
    /// <summary>
    /// مسیر فایل اسکن شده (اگر از طریق اسکن باشد)
    /// </summary>
    public string? ScannedFilePath { get; set; }
    
    /// <summary>
    /// آدرس ایمیل فرستنده (اگر از طریق ایمیل باشد)
    /// </summary>
    public string? SenderEmail { get; set; }
    
    /// <summary>
    /// شناسه کاربر ثبت‌کننده
    /// </summary>
    public int RegisteredByUserId { get; set; }
    public virtual User RegisteredByUser { get; set; } = null!;
    
    /// <summary>
    /// تاریخ و ساعت ثبت
    /// </summary>
    public DateTime RegisteredDateTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// شناسه نوع مدرک
    /// </summary>
    public int? DocumentTypeId { get; set; }
    public virtual DocumentType? DocumentType { get; set; }
    
    /// <summary>
    /// شناسه طبقه‌بندی
    /// </summary>
    public int? ClassificationId { get; set; }
    public virtual Classification? Classification { get; set; }
    
    /// <summary>
    /// شناسه نحوه دریافت
    /// </summary>
    public int? ReceiptMethodId { get; set; }
    public virtual ReceiptMethod? ReceiptMethodEntity { get; set; }
    
    /// <summary>
    /// شناسه فوریت
    /// </summary>
    public int? UrgencyId { get; set; }
    public virtual Urgency? Urgency { get; set; }
    
    /// <summary>
    /// تاریخ مدرک
    /// </summary>
    public DateTime? DocumentDate { get; set; }
    
    /// <summary>
    /// شماره اولیه (شماره نامه فرستنده)
    /// </summary>
    public string? InitialNumber { get; set; }
    
    /// <summary>
    /// آدرس فرستنده
    /// </summary>
    public string? SenderAddress { get; set; }
    
    /// <summary>
    /// موبایل فرستنده
    /// </summary>
    public string? SenderMobile { get; set; }
    
    /// <summary>
    /// شناسه دبیرخانه
    /// </summary>
    public int? SecretariatId { get; set; }
    public virtual Secretariat? Secretariat { get; set; }
}



