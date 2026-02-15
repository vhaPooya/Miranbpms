namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای پیام فکس
/// </summary>
public class FaxMessageDto
{
    public int Id { get; set; }
    public int? DocumentId { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public string? FromFaxNumber { get; set; }
    public string? FromName { get; set; }
    public string ToFaxNumber { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public string? FilePath { get; set; }
    public int? PageCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? SentReceivedDateTime { get; set; }
    public DateTime CreatedDateTime { get; set; }
}




