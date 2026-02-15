namespace Automation.Core.Entities;

/// <summary>
/// تنظیمات کاربر
/// </summary>
public class UserSetting : BaseEntity
{
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string? Category { get; set; }
}
