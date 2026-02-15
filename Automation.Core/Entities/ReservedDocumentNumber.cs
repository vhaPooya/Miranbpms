namespace Automation.Core.Entities;

/// <summary>
/// شماره‌های رزرو شده برای مدارک
/// </summary>
public class ReservedDocumentNumber : BaseEntity
{
    /// <summary>
    /// شماره رزرو شده
    /// </summary>
    public string ReservedNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// نوع مدرک (INCOMING, OUTGOING)
    /// </summary>
    public string DocumentType { get; set; } = "INCOMING";
    
    /// <summary>
    /// شناسه دبیرخانه
    /// </summary>
    public int SecretariatId { get; set; }
    public virtual Secretariat Secretariat { get; set; } = null!;
    
    /// <summary>
    /// شناسه کاربر رزروکننده
    /// </summary>
    public int ReservedByUserId { get; set; }
    public virtual User ReservedByUser { get; set; } = null!;
    
    /// <summary>
    /// تاریخ رزرو
    /// </summary>
    public DateTime ReservedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// تاریخ انقضا (اختیاری)
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
    
    /// <summary>
    /// آیا استفاده شده است؟
    /// </summary>
    public bool IsUsed { get; set; } = false;
    
    /// <summary>
    /// شناسه مدرک استفاده‌کننده (در صورت استفاده)
    /// </summary>
    public int? UsedByDocumentId { get; set; }
    public virtual Document? UsedByDocument { get; set; }
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
}



