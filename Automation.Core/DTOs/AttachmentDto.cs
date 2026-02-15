using System.ComponentModel.DataAnnotations;

namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ضمائم
/// </summary>
public class AttachmentDto
{
    [StringLength(500, ErrorMessage = "نام فایل نمی‌تواند بیشتر از 500 کاراکتر باشد")]
    public string? FileName { get; set; }
    
    [StringLength(1000, ErrorMessage = "مسیر فایل نمی‌تواند بیشتر از 1000 کاراکتر باشد")]
    public string? FilePath { get; set; }
    
    [Range(0, long.MaxValue, ErrorMessage = "حجم فایل باید مثبت باشد")]
    public long? FileSize { get; set; }
    
    [StringLength(100, ErrorMessage = "نوع MIME نمی‌تواند بیشتر از 100 کاراکتر باشد")]
    public string? MimeType { get; set; }
    
    [StringLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 1000 کاراکتر باشد")]
    public string? Description { get; set; }
    
    public int? ReferencedDocumentId { get; set; }
    public int? ReferencedFormId { get; set; }
}




