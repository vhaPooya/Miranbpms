namespace Automation.Core.DTOs;

/// <summary>
/// نتیجه ثبت صادره
/// </summary>
public class RegisterOutgoingResult
{
    public bool Success { get; set; }
    public int? DocumentId { get; set; }
    public string? OutgoingNumber { get; set; }
    public string? ErrorMessage { get; set; }
}




