using System.ComponentModel.DataAnnotations;

namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ارسال ایمیل
/// </summary>
public class SendEmailDto
{
    [Required(ErrorMessage = "شناسه تنظیمات ایمیل الزامی است")]
    public int SettingsId { get; set; }
    
    [Required(ErrorMessage = "آدرس ایمیل گیرنده الزامی است")]
    [EmailAddress(ErrorMessage = "آدرس ایمیل معتبر نیست")]
    public string ToEmail { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "موضوع الزامی است")]
    [StringLength(500, ErrorMessage = "موضوع نمی‌تواند بیشتر از 500 کاراکتر باشد")]
    public string Subject { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "محتوا الزامی است")]
    public string Body { get; set; } = string.Empty;
    
    public bool IsHtml { get; set; } = false;
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public List<string>? Attachments { get; set; }
    public int? DocumentId { get; set; }
}





