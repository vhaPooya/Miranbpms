using System.ComponentModel.DataAnnotations;

namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ثبت وارده
/// </summary>
public class RegisterIncomingDto
{
    [Required(ErrorMessage = "شناسه فرم الزامی است")]
    public int FormId { get; set; }
    
    [Required(ErrorMessage = "موضوع الزامی است")]
    [StringLength(500, ErrorMessage = "موضوع نمی‌تواند بیشتر از 500 کاراکتر باشد")]
    public string Subject { get; set; } = string.Empty;
    
    public string Content { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "نام فرستنده الزامی است")]
    [StringLength(500, ErrorMessage = "نام فرستنده نمی‌تواند بیشتر از 500 کاراکتر باشد")]
    public string SenderName { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "شماره فرستنده نمی‌تواند بیشتر از 100 کاراکتر باشد")]
    public string? SenderNumber { get; set; }
    
    public DateTime? SenderDate { get; set; }
    
    [StringLength(50, ErrorMessage = "روش دریافت نمی‌تواند بیشتر از 50 کاراکتر باشد")]
    public string ReceiptMethod { get; set; } = "MANUAL"; // SCAN, EMAIL, MANUAL, SYSTEM
    
    [StringLength(1000, ErrorMessage = "مسیر فایل اسکن نمی‌تواند بیشتر از 1000 کاراکتر باشد")]
    public string? ScannedFilePath { get; set; }
    
    [EmailAddress(ErrorMessage = "آدرس ایمیل معتبر نیست")]
    [StringLength(255, ErrorMessage = "آدرس ایمیل نمی‌تواند بیشتر از 255 کاراکتر باشد")]
    public string? SenderEmail { get; set; }
    
    [Required(ErrorMessage = "شناسه کاربر ثبت‌کننده الزامی است")]
    public int RegisteredByUserId { get; set; }
    
    public int? ConfidentialityLevelId { get; set; }
    public int? PriorityId { get; set; }
    
    public DateTime? IncomingDate { get; set; }
    
    // فیلدهای جدید
    public int? DocumentTypeId { get; set; }
    public int? ClassificationId { get; set; }
    public int? ReceiptMethodId { get; set; }
    public int? UrgencyId { get; set; }
    
    public DateTime? DocumentDate { get; set; }
    
    [StringLength(100, ErrorMessage = "شماره اولیه نمی‌تواند بیشتر از 100 کاراکتر باشد")]
    public string? InitialNumber { get; set; }
    
    [StringLength(100, ErrorMessage = "شماره وارده نمی‌تواند بیشتر از 100 کاراکتر باشد")]
    public string? IncomingNumber { get; set; }
    
    [StringLength(1000, ErrorMessage = "آدرس فرستنده نمی‌تواند بیشتر از 1000 کاراکتر باشد")]
    public string? SenderAddress { get; set; }
    
    [StringLength(20, ErrorMessage = "شماره موبایل نمی‌تواند بیشتر از 20 کاراکتر باشد")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل باید به فرمت 09xxxxxxxxx باشد")]
    public string? SenderMobile { get; set; }
    
    [StringLength(1000, ErrorMessage = "کلیدواژه‌ها نمی‌توانند بیشتر از 1000 کاراکتر باشند")]
    public string? Keywords { get; set; }
    
    public List<CopyRecipientDto>? Copies { get; set; }
    public List<AttachmentDto>? Attachments { get; set; }
    public List<AttachmentDto>? FollowUps { get; set; }
    public int? SecretariatId { get; set; }
    public int? OrganizationId { get; set; }
    public int? DepartmentId { get; set; }
}




