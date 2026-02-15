namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای رزرو شماره
/// </summary>
public class ReserveNumberDto
{
    public string ReservedNumber { get; set; } = string.Empty;
    public string DocumentType { get; set; } = "INCOMING";
    public int SecretariatId { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Description { get; set; }
}





