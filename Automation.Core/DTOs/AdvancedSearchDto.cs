namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای جستجوی پیشرفته
/// </summary>
public class AdvancedSearchDto
{
    public string? Query { get; set; }
    public int? FormId { get; set; }
    public Dictionary<string, object>? FieldFilters { get; set; }
    public List<string>? DocumentTypes { get; set; }
    public List<string>? Statuses { get; set; }
    public int? CreatedByUserId { get; set; }
    public int? OrganizationId { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? IsSigned { get; set; }
    public bool? IsArchived { get; set; }
    public int? ConfidentialityLevelId { get; set; }
    public int? PriorityId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}




