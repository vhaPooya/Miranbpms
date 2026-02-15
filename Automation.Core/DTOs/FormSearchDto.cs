namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای جستجوی فرم
/// </summary>
public class FormSearchDto
{
    public string? Query { get; set; }
    public int? FormId { get; set; }
    public Dictionary<string, object>? FieldFilters { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}




