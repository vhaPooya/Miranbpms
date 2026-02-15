namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای جستجوی سراسری
/// </summary>
public class GlobalSearchDto
{
    public string Query { get; set; } = string.Empty;
    public List<string>? DocumentTypes { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? FormId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}




