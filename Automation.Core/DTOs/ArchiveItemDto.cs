namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای آیتم بایگانی
/// </summary>
public class ArchiveItemDto
{
    public int DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string? IncomingNumber { get; set; }
    public string? OutgoingNumber { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateTime CreatedDateTime { get; set; }
    public DateTime ArchivedDateTime { get; set; }
    public string? ArchiveLocation { get; set; }
}




