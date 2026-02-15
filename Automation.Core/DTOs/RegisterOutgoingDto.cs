using System.ComponentModel.DataAnnotations;

namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ثبت صادره
/// </summary>
public class RegisterOutgoingDto
{
    [Required(ErrorMessage = "شناسه فرم الزامی است")]
    public int FormId { get; set; }
    
    [Required(ErrorMessage = "موضوع الزامی است")]
    [StringLength(500, ErrorMessage = "موضوع نمی‌تواند بیشتر از 500 کاراکتر باشد")]
    public string Subject { get; set; } = string.Empty;
    
    public string Content { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "نام گیرنده الزامی است")]
    [StringLength(500, ErrorMessage = "نام گیرنده نمی‌تواند بیشتر از 500 کاراکتر باشد")]
    public string RecipientName { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "آدرس گیرنده نمی‌تواند بیشتر از 1000 کاراکتر باشد")]
    public string? RecipientAddress { get; set; }
    
    [StringLength(50, ErrorMessage = "روش ارسال نمی‌تواند بیشتر از 50 کاراکتر باشد")]
    public string DeliveryMethod { get; set; } = "SYSTEM"; // POST, EMAIL, FAX, HAND_DELIVERY, SYSTEM
    
    [Required(ErrorMessage = "شناسه کاربر ثبت‌کننده الزامی است")]
    public int RegisteredByUserId { get; set; }
    
    public int? ConfidentialityLevelId { get; set; }
    public int? PriorityId { get; set; }
    public DateTime? OutgoingDate { get; set; }
    
    // فیلدهای جدید
    public int? DocumentTypeId { get; set; }
    public int? ClassificationId { get; set; }
    public int? UrgencyId { get; set; }
    public DateTime? DocumentDate { get; set; }
    
    [StringLength(100, ErrorMessage = "شماره اولیه نمی‌تواند بیشتر از 100 کاراکتر باشد")]
    public string? InitialNumber { get; set; }
    
    [StringLength(100, ErrorMessage = "شماره صادره نمی‌تواند بیشتر از 100 کاراکتر باشد")]
    public string? OutgoingNumber { get; set; }
    
    [StringLength(1000, ErrorMessage = "کلیدواژه‌ها نمی‌توانند بیشتر از 1000 کاراکتر باشند")]
    public string? Keywords { get; set; }
    
    public List<CopyRecipientDto>? Copies { get; set; }
    public List<AttachmentDto>? Attachments { get; set; }
    public List<AttachmentDto>? FollowUps { get; set; }
    public int? SecretariatId { get; set; }
    public int? OrganizationId { get; set; }
    public int? DepartmentId { get; set; }
}




