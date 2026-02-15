namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ایجاد نحوه دریافت
/// </summary>
public class CreateReceiptMethodDto
{
    public string ReceiptMethodCode { get; set; } = string.Empty;
    public string ReceiptMethodName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
}





