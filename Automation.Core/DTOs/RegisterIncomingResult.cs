namespace Automation.Core.DTOs;

/// <summary>
/// نتیجه ثبت وارده
/// </summary>
public class RegisterIncomingResult
{
    public bool Success { get; set; }
    public int? DocumentId { get; set; }
    public string? IncomingNumber { get; set; }
    public string? ErrorMessage { get; set; }
}




