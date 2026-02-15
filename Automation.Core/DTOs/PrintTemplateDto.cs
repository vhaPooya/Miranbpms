namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای قالب چاپ
/// </summary>
public class PrintTemplateDto
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? FilePath { get; set; }
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }
}





