namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای فیلد فرم
/// </summary>
public class FormFieldDto
{
    public int Id { get; set; }
    public int FieldTypeId { get; set; }
    public int? ParentFieldId { get; set; }
    public string FieldKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LabelFa { get; set; } = string.Empty;
    public string? LabelEn { get; set; }
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public string? DefaultValue { get; set; }
    public string? DatabaseColumnName { get; set; }
    public string? DatabaseColumnType { get; set; }
    public bool IsRequired { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsDisabled { get; set; }
    public bool IsHidden { get; set; }
    public int DisplayOrder { get; set; }
    public string? Properties { get; set; }
    public string? Styles { get; set; }
    public string? CssClasses { get; set; }
    public string? InlineStyles { get; set; }
    public string? Events { get; set; }
    public string? RenderCondition { get; set; }
    public string? Options { get; set; }
    public string? DataSourceUrl { get; set; }
    public int? MaxLength { get; set; }
    public bool IsNullable { get; set; } = true;
    public bool HasIndex { get; set; } = false;
    public string? CustomCss { get; set; }
    public string? CustomJs { get; set; }
}





