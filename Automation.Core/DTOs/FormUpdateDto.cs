using Automation.Core.Entities;

namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای به‌روزرسانی فرم
/// </summary>
public class FormUpdateDto
{
    public string? NameFa { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public FormLayoutType? LayoutType { get; set; }
    public string? NumberingRule { get; set; }
    public string? NumberingPrefix { get; set; }
    public string? CanvasSize { get; set; }
    public string? BackgroundSettings { get; set; }
    public string? CustomStyles { get; set; }
    public string? CustomScripts { get; set; }
    public object? DesignData { get; set; }
    public object? FormSettings { get; set; }
    public bool? UpdateTable { get; set; }
}





