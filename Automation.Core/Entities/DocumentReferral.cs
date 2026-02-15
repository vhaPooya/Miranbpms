namespace Automation.Core.Entities;

/// <summary>
/// ارجاع‌های مدارک
/// </summary>
public class DocumentReferral : BaseEntity
{
    /// <summary>
    /// شناسه مدرک/فرم
    /// </summary>
    public int DocumentId { get; set; }
    public virtual Document Document { get; set; } = null!;
    
    /// <summary>
    /// شناسه فرم
    /// </summary>
    public int FormId { get; set; }
    public virtual Form Form { get; set; } = null!;
    
    /// <summary>
    /// شناسه نوع عملگر (Action Type)
    /// </summary>
    public int ActionTypeId { get; set; }
    public virtual ActionType ActionType { get; set; } = null!;

    /// <summary>
    /// کد نوع عمل به صورت رشته (سازگاری)
    /// </summary>
    public string? ActionTypeCode { get; set; }
    
    /// <summary>
    /// شناسه کاربر گیرنده (Nullable - اگر به کاربر خاص ارجاع شود)
    /// </summary>
    public int? ReferredToUserId { get; set; }
    public virtual User? ReferredToUser { get; set; }
    
    /// <summary>
    /// شناسه نقش گیرنده (Nullable - اگر به نقش خاص ارجاع شود)
    /// </summary>
    public int? ReferredToRoleId { get; set; }
    public virtual Role? ReferredToRole { get; set; }
    
    /// <summary>
    /// شناسه بخش گیرنده (Nullable - اگر به بخش خاص ارجاع شود)
    /// </summary>
    public int? ReferredToDepartmentId { get; set; }
    public virtual Department? ReferredToDepartment { get; set; }
    
    /// <summary>
    /// شناسه گروه گیرنده (Nullable - اگر به گروه خاص ارجاع شود)
    /// </summary>
    public int? ReferredToGroupId { get; set; }
    public virtual Group? ReferredToGroup { get; set; }
    
    /// <summary>
    /// شناسه کاربر ارجاع‌دهنده
    /// </summary>
    public int ReferredByUserId { get; set; }
    public virtual User ReferredByUser { get; set; } = null!;

    /// <summary>
    /// کاربر مبدا (همان ReferredByUserId - سازگاری)
    /// </summary>
    public int FromUserId { get => ReferredByUserId; set => ReferredByUserId = value; }

    /// <summary>
    /// کاربر مقصد (همان ReferredToUserId - سازگاری)
    /// </summary>
    public int? ToUserId { get => ReferredToUserId; set => ReferredToUserId = value; }

    /// <summary>
    /// تاریخ ارجاع
    /// </summary>
    public DateTime ReferredDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// تاریخ ارجاع (نام دیگر - سازگاری)
    /// </summary>
    public DateTime ReferralDate { get => ReferredDate; set => ReferredDate = value; }
    
    /// <summary>
    /// تاریخ سررسید (Nullable)
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// وضعیت (PENDING, COMPLETED, CANCELLED)
    /// </summary>
    public string Status { get; set; } = "PENDING";
    
    /// <summary>
    /// یادداشت‌ها
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// آیا خوانده شده است؟
    /// </summary>
    public bool IsRead { get; set; } = false;
    
    /// <summary>
    /// تاریخ خواندن
    /// </summary>
    public DateTime? ReadDate { get; set; }
    
    /// <summary>
    /// تاریخ تکمیل
    /// </summary>
    public DateTime? CompletedDate { get; set; }
}



