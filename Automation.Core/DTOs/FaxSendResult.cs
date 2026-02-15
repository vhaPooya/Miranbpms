namespace Automation.Core.DTOs;

/// <summary>
/// نتیجه ارسال فکس
/// </summary>
public class FaxSendResult
{
    public bool Success { get; set; }
    public int? FaxMessageId { get; set; }
    public string? ErrorMessage { get; set; }
}




