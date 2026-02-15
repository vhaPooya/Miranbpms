namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای لیست بایگانی
/// </summary>
public class ArchiveListDto
{
    public List<ArchiveItemDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}




