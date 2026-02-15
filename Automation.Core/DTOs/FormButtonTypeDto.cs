namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای نوع دکمه پیشفرض
/// </summary>
public class FormButtonTypeDto
{
    public int Id { get; set; }
    public string ButtonCode { get; set; } = string.Empty;
    public string NameFa { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? DefaultIcon { get; set; }
    public string? DefaultColor { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ActionHandler { get; set; }
    public bool OpensModal { get; set; }
    public string? ModalId { get; set; }
    public int DisplayOrder { get; set; }
    public string? Description { get; set; }
}



