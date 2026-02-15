namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای آیتم نتیجه جستجو
/// </summary>
public class SearchResultItemDto
{
    public int DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string? IncomingNumber { get; set; }
    public string? OutgoingNumber { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDateTime { get; set; }
    public string? CreatedByUserName { get; set; }
    public string? FormName { get; set; }
    public string? Snippet { get; set; } // بخشی از محتوا که با جستجو مطابقت دارد
    public double? RelevanceScore { get; set; }
}




