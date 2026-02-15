using System.ComponentModel.DataAnnotations;

namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای گیرندگان رونوشت
/// </summary>
public class CopyRecipientDto
{
    [Required(ErrorMessage = "نام گیرنده الزامی است")]
    [StringLength(500, ErrorMessage = "نام گیرنده نمی‌تواند بیشتر از 500 کاراکتر باشد")]
    public string RecipientName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "نوع اقدام الزامی است")]
    [StringLength(50, ErrorMessage = "نوع اقدام نمی‌تواند بیشتر از 50 کاراکتر باشد")]
    public string ActionType { get; set; } = "FOR_INFORMATION"; // جهت اطلاع، جهت اقدام، جهت پیگیری
    
    public int DisplayOrder { get; set; }
}




