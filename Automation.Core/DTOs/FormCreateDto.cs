using Automation.Core.Entities;

namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ایجاد فرم
/// </summary>
public class FormCreateDto
{
    public string? FormCode { get; set; }
    public string NameFa { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public FormLayoutType LayoutType { get; set; } = FormLayoutType.Flex;
    public string? NumberingRule { get; set; }
    public string? NumberingPrefix { get; set; }
    public string? CanvasSize { get; set; }
    public string? BackgroundSettings { get; set; }
    public string? DatabaseTableName { get; set; }
    public object? DesignData { get; set; }
    public string? CustomScripts { get; set; }
    public string? CustomStyles { get; set; }
    public object? FormSettings { get; set; }
    public bool? CreateTable { get; set; }
}





