namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای قالب استایل دکمه
/// </summary>
public class ButtonStylePresetDto
{
    public int Id { get; set; }
    public string PresetName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string StyleDefinition { get; set; } = "{}";
    public bool IsSystem { get; set; }
    public int DisplayOrder { get; set; }
}



