namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای لیست کارتابل
/// </summary>
public class CabinetListDto
{
    public List<CabinetItemDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}




