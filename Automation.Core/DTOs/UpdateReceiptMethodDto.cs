namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای به‌روزرسانی نحوه دریافت
/// </summary>
public class UpdateReceiptMethodDto
{
    public string ReceiptMethodCode { get; set; } = string.Empty;
    public string ReceiptMethodName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
}





