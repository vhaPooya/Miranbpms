namespace Automation.Core.DTOs;

/// <summary>
/// نتیجه ارسال ایمیل
/// </summary>
public class EmailSendResult
{
    public bool Success { get; set; }
    public int? EmailMessageId { get; set; }
    public string? ErrorMessage { get; set; }
}




