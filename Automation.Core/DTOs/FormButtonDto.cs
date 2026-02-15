namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای دکمه‌های یک فرم
/// </summary>
public class FormButtonDto
{
    public int? Id { get; set; }
    public int FormId { get; set; }
    public int? FormButtonTypeId { get; set; }
    public bool IsProcessServiceButton { get; set; }
    public string? CustomTitleFa { get; set; }
    public string? CustomTitleEn { get; set; }
    public int DisplayOrder { get; set; }
    public string DisplayMode { get; set; } = "icon_and_title";
    public string? CustomIcon { get; set; }
    public string StyleSettings { get; set; } = "{}";
    public int? ButtonStylePresetId { get; set; }
    public int? WorkflowServiceId { get; set; }
    public int? InheritFromButtonTypeId { get; set; }
    public string? ExecutionTiming { get; set; }
    public string? ProcessServiceConfig { get; set; }
    public string? VisibilityCondition { get; set; }
    public string? RequiredPermission { get; set; }
    public string? ConfirmationMessage { get; set; }
    public bool IsEnabled { get; set; } = true;

    // Related entity data (loaded from joins)
    public string? ButtonTypeName { get; set; }
    public string? ButtonTypeIcon { get; set; }
    public string? ButtonTypeColor { get; set; }
    public string? ButtonTypeActionHandler { get; set; }
    public bool? ButtonTypeOpensModal { get; set; }
    public string? ButtonTypeModalId { get; set; }
    public string? PresetName { get; set; }
}



