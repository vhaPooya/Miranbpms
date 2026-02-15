namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای پیام ایمیل
/// </summary>
public class EmailMessageDto
{
    public int Id { get; set; }
    public int? DocumentId { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string? FromName { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? SentReceivedDateTime { get; set; }
    public DateTime CreatedDateTime { get; set; }
}




