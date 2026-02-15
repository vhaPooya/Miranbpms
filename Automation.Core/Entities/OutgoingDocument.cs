namespace Automation.Core.Entities;

/// <summary>
/// نامه‌های صادره
/// </summary>
public class OutgoingDocument : BaseEntity
{
    /// <summary>
    /// شناسه مدرک پایه
    /// </summary>
    public int DocumentId { get; set; }
    public virtual Document Document { get; set; } = null!;
    
    /// <summary>
    /// شماره صادره (از دفتر)
    /// </summary>
    public string OutgoingNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// تاریخ صادره
    /// </summary>
    public DateTime OutgoingDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// گیرنده (نام شخص/سازمان)
    /// </summary>
    public string RecipientName { get; set; } = string.Empty;
    
    /// <summary>
    /// آدرس گیرنده
    /// </summary>
    public string? RecipientAddress { get; set; }
    
    /// <summary>
    /// روش ارسال (POST, EMAIL, FAX, HAND_DELIVERY, SYSTEM)
    /// </summary>
    public string DeliveryMethod { get; set; } = "SYSTEM";
    
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
    /// تاریخ و ساعت ارسال
    /// </summary>
    public DateTime? SentDateTime { get; set; }
    
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
    /// شناسه فوریت
    /// </summary>
    public int? UrgencyId { get; set; }
    public virtual Urgency? Urgency { get; set; }
    
    /// <summary>
    /// تاریخ مدرک
    /// </summary>
    public DateTime? DocumentDate { get; set; }
    
    /// <summary>
    /// شماره اولیه
    /// </summary>
    public string? InitialNumber { get; set; }
    
    /// <summary>
    /// شناسه دبیرخانه
    /// </summary>
    public int? SecretariatId { get; set; }
    public virtual Secretariat? Secretariat { get; set; }
}



