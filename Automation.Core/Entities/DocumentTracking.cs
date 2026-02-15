namespace Automation.Core.Entities;

/// <summary>
/// ردیابی و تاریخچه مدرک (برای نمایش کامل گردش)
/// </summary>
public class DocumentTracking : BaseEntity
{
    /// <summary>
    /// شناسه مدرک
    /// </summary>
    public int DocumentId { get; set; }
    public virtual Document Document { get; set; } = null!;
    
    /// <summary>
    /// شناسه کاربر (ایجادکننده، گیرنده، یا مشاهده‌کننده)
    /// </summary>
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// شناسه سمت
    /// </summary>
    public int? PositionId { get; set; }
    public virtual Position? Position { get; set; }

    /// <summary>
    /// تاریخ دریافت
    /// </summary>
    public DateTime? ReceivedDate { get; set; }

    /// <summary>
    /// تاریخ اولین مشاهده
    /// </summary>
    public DateTime? FirstViewDate { get; set; }

    /// <summary>
    /// تاریخ تکمیل
    /// </summary>
    public DateTime? CompletionDate { get; set; }
    
    /// <summary>
    /// نوع عملیات (CREATED, RECEIVED, VIEWED, REVIEWED, APPROVED, REJECTED, FORWARDED, ARCHIVED)
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// وضعیت (همان ActionType - سازگاری)
    /// </summary>
    public string Status { get => ActionType; set => ActionType = value; }

    /// <summary>
    /// عملیات انجام‌شده (همان ActionType - سازگاری)
    /// </summary>
    public string ActionPerformed { get => ActionType; set => ActionType = value; }

    /// <summary>
    /// تاریخ انجام (همان ActionDateTime - سازگاری)
    /// </summary>
    public DateTime PerformedDate { get => ActionDateTime; set => ActionDateTime = value; }
    
    /// <summary>
    /// تاریخ و ساعت عملیات
    /// </summary>
    public DateTime ActionDateTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// تاریخ و ساعت اولین مشاهده (اگر ActionType = VIEWED)
    /// </summary>
    public DateTime? FirstViewedDateTime { get; set; }
    
    /// <summary>
    /// آیا بررسی شده است؟
    /// </summary>
    public bool IsReviewed { get; set; } = false;
    
    /// <summary>
    /// تاریخ و ساعت بررسی
    /// </summary>
    public DateTime? ReviewedDateTime { get; set; }
    
    /// <summary>
    /// آیا در دست بررسی است؟
    /// </summary>
    public bool IsUnderReview { get; set; } = false;
    
    /// <summary>
    /// تاریخ شروع بررسی
    /// </summary>
    public DateTime? ReviewStartDateTime { get; set; }
    
    /// <summary>
    /// توضیحات/یادداشت
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// شناسه ارجاع مرتبط (اگر از طریق ارجاع باشد)
    /// </summary>
    public int? ReferralId { get; set; }
    public virtual DocumentReferral? Referral { get; set; }
    
}



