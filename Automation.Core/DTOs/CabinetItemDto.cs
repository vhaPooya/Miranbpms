namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای آیتم کارتابل
/// </summary>
public class CabinetItemDto
{
    public int DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDateTime { get; set; }
    public DateTime? ReceivedDateTime { get; set; }
    public DateTime? FirstViewedDateTime { get; set; }
    public bool IsViewed { get; set; }
    public bool IsReviewed { get; set; }
    public bool IsUnderReview { get; set; }
    public string? SenderName { get; set; }
    public string? RecipientName { get; set; }
    public string? Priority { get; set; }
    public string? ConfidentialityLevel { get; set; }
    public int AttachmentCount { get; set; }
    public string? ActionType { get; set; }
    public string? ActionTypeName { get; set; }
    public bool CanEdit { get; set; }
}




