namespace Automation.Core.Entities;

/// <summary>
/// پیوست‌ها و عطف‌های مدارک
/// </summary>
public class DocumentAttachment : BaseEntity
{
    /// <summary>
    /// شناسه مدرک/فرم
    /// </summary>
    public int DocumentId { get; set; }
    
    /// <summary>
    /// شناسه فرم (برای ارتباط با Form)
    /// </summary>
    public int FormId { get; set; }
    public virtual Form Form { get; set; } = null!;
    
    /// <summary>
    /// نوع پیوست (EXTERNAL_FILE یا INTERNAL_DOCUMENT)
    /// </summary>
    public string AttachmentType { get; set; } = string.Empty;
    
    /// <summary>
    /// دسته‌بندی (ATTACHMENT یا REFERENCE)
    /// </summary>
    public string AttachmentCategory { get; set; } = "ATTACHMENT"; // ATTACHMENT یا REFERENCE
    
    // برای فایل خارجی
    /// <summary>
    /// نام فایل
    /// </summary>
    public string? FileName { get; set; }
    
    /// <summary>
    /// مسیر فایل
    /// </summary>
    public string? FilePath { get; set; }
    
    /// <summary>
    /// حجم فایل (بایت)
    /// </summary>
    public long? FileSize { get; set; }
    
    /// <summary>
    /// نوع MIME
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// نوع محتوا (همان MimeType - سازگاری)
    /// </summary>
    public string? ContentType { get => MimeType; set => MimeType = value; }

    /// <summary>
    /// مسیر ذخیره (همان FilePath - سازگاری)
    /// </summary>
    public string? StoragePath { get => FilePath; set => FilePath = value; }

    /// <summary>
    /// شناسه کاربر آپلودکننده (همان UploadedByUserId - سازگاری)
    /// </summary>
    public int UploadedById { get => UploadedByUserId; set => UploadedByUserId = value; }

    /// <summary>
    /// تاریخ آپلود (همان UploadedDate - سازگاری)
    /// </summary>
    public DateTime UploadDate { get => UploadedDate; set => UploadedDate = value; }
    
    // برای مدرک داخلی
    /// <summary>
    /// شناسه مدرک مرجع (برای مدرک داخلی)
    /// </summary>
    public int? ReferencedDocumentId { get; set; }
    
    /// <summary>
    /// شناسه فرم مرجع (برای مدرک داخلی)
    /// </summary>
    public int? ReferencedFormId { get; set; }
    public virtual Form? ReferencedForm { get; set; }
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// شناسه کاربر آپلودکننده
    /// </summary>
    public int UploadedByUserId { get; set; }
    public virtual User UploadedByUser { get; set; } = null!;
    
    /// <summary>
    /// تاریخ آپلود
    /// </summary>
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
}



