namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای جستجوی مدرک
/// </summary>
public class DocumentSearchDto
{
    public string? DocumentNumber { get; set; }
    public string? Subject { get; set; }
    public string? Content { get; set; }
    public List<string>? DocumentTypes { get; set; }
    public List<string>? Statuses { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? ConfidentialityLevelId { get; set; }
    public int? PriorityId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}




