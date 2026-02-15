namespace Automation.Core.Entities;

/// <summary>
/// مدارک و نامه‌های سیستم (پایه برای همه انواع مدارک)
/// </summary>
public class Document : BaseEntity
{
    /// <summary>
    /// شناسه فرم مرتبط
    /// </summary>
    public int FormId { get; set; }
    public virtual Form Form { get; set; } = null!;
    
    /// <summary>
    /// شماره مدرک (منحصر به فرد)
    /// </summary>
    public string DocumentNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// نوع مدرک (INCOMING, OUTGOING, INTERNAL, DRAFT)
    /// </summary>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// شناسه نوع مدرک
    /// </summary>
    public int? DocumentTypeId { get; set; }

    /// <summary>
    /// دسته‌بندی / طبقه‌بندی
    /// </summary>
    public string? Classification { get; set; }

    /// <summary>
    /// شناسه کاربر ایجادکننده (همان CreatedByUserId)
    /// </summary>
    public int CreatedById { get => CreatedByUserId; set => CreatedByUserId = value; }

    /// <summary>
    /// شناسه دبیرخانه
    /// </summary>
    public int? SecretariatId { get; set; }

    /// <summary>
    /// مهلت
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// تاریخ ویرایش
    /// </summary>
    public DateTime? ModifiedDate { get => EditDate; set => EditDate = value; }

    /// <summary>
    /// شناسه کاربر تاییدکننده
    /// </summary>
    public int? ApprovedById { get; set; }

    /// <summary>
    /// تاریخ تایید
    /// </summary>
    public DateTime? ApprovedDate { get; set; }

    /// <summary>
    /// شناسه سند والد (برای نسخه‌ها)
    /// </summary>
    public int? ParentDocumentId { get; set; }

    /// <summary>
    /// شماره نسخه
    /// </summary>
    public int? Version { get; set; }
    
    /// <summary>
    /// موضوع
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// محتویات مدرک (JSON)
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// شناسه کاربر ایجادکننده
    /// </summary>
    public int CreatedByUserId { get; set; }
    public virtual User CreatedByUser { get; set; } = null!;
    
    /// <summary>
    /// تاریخ و ساعت ایجاد
    /// </summary>
    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// آیا امضاء شده است؟
    /// </summary>
    public bool IsSigned { get; set; } = false;
    
    /// <summary>
    /// تاریخ و ساعت امضاء
    /// </summary>
    public DateTime? SignedDateTime { get; set; }
    
    /// <summary>
    /// شناسه کاربر امضاءکننده
    /// </summary>
    public int? SignedByUserId { get; set; }
    public virtual User? SignedByUser { get; set; }

    /// <summary>
    /// مسیر فایل امضای استفاده شده
    /// </summary>
    public string? SignaturePath { get; set; }

    /// <summary>
    /// آیا بایگانی شده است؟
    /// </summary>
    public bool IsArchived { get; set; } = false;
    
    /// <summary>
    /// تاریخ بایگانی
    /// </summary>
    public DateTime? ArchivedDateTime { get; set; }

    /// <summary>
    /// شناسه کاربر بایگانی‌کننده
    /// </summary>
    public int? ArchivedById { get; set; }

    /// <summary>
    /// تاریخ بایگانی (نام دیگر)
    /// </summary>
    public DateTime? ArchivedDate { get => ArchivedDateTime; set => ArchivedDateTime = value; }

    /// <summary>
    /// شناسه پوشه بایگانی
    /// </summary>
    public int? ArchiveFolderId { get; set; }
    
    /// <summary>
    /// وضعیت (DRAFT, PENDING, IN_PROGRESS, COMPLETED, ARCHIVED, CANCELLED)
    /// </summary>
    public string Status { get; set; } = "DRAFT";
    
    /// <summary>
    /// سطح محرمانگی
    /// </summary>
    public int? ConfidentialityLevelId { get; set; }
    public virtual ConfidentialityLevel? ConfidentialityLevel { get; set; }
    
    /// <summary>
    /// اولویت
    /// </summary>
    public int? PriorityId { get; set; }
    public virtual Priority? Priority { get; set; }

    /// <summary>
    /// کد اولویت به صورت رشته (سازگاری)
    /// </summary>
    public string? PriorityCode { get; set; }
    
    /// <summary>
    /// تاریخچه ردیابی
    /// </summary>
    public virtual ICollection<DocumentTracking> TrackingHistory { get; set; } = new List<DocumentTracking>();

    /// <summary>
    /// تاریخچه ردیابی (نام دیگر - سازگاری)
    /// </summary>
    public virtual ICollection<DocumentTracking> DocumentTrackings { get => TrackingHistory; set => TrackingHistory = value; }
    
    /// <summary>
    /// ارجاع‌ها
    /// </summary>
    public virtual ICollection<DocumentReferral> Referrals { get; set; } = new List<DocumentReferral>();
    
    /// <summary>
    /// پیوست‌ها
    /// </summary>
    public virtual ICollection<DocumentAttachment> Attachments { get; set; } = new List<DocumentAttachment>();
    
    /// <summary>
    /// کلیدواژه‌ها
    /// </summary>
    public virtual ICollection<DocumentKeyword> Keywords { get; set; } = new List<DocumentKeyword>();
    
    /// <summary>
    /// گیرندگان رونوشت
    /// </summary>
    public virtual ICollection<DocumentCopyRecipient> CopyRecipients { get; set; } = new List<DocumentCopyRecipient>();
}



