namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای جزئیات مدرک در کارتابل
/// </summary>
public class CabinetDocumentDto
{
    public int DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsSigned { get; set; }
    public bool CanEdit { get; set; }
    public List<DocumentAttachmentDto> Attachments { get; set; } = new();
    public List<DocumentReferralDto> Referrals { get; set; } = new();
}




