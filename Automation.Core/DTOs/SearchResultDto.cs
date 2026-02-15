namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای نتیجه جستجو
/// </summary>
public class SearchResultDto
{
    public List<SearchResultItemDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public TimeSpan SearchDuration { get; set; }
}




