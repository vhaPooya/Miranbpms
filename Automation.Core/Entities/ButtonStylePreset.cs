namespace Automation.Core.Entities;

/// <summary>
/// قالب‌های استایل آماده برای دکمه‌های فرم
/// </summary>
public class ButtonStylePreset : BaseEntity
{
    public string PresetName { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>تنظیمات استایل به صورت JSON (color, bgColor, fontFamily, fontSize, borderRadius, padding, boxShadow)</summary>
    public string StyleDefinition { get; set; } = "{}";

    /// <summary>آیا قالب سیستمی است (غیرقابل حذف)</summary>
    public bool IsSystem { get; set; }

    public int DisplayOrder { get; set; }
}


