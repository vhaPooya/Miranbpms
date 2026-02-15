namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای به‌روزرسانی تنظیمات فکس
/// </summary>
public class UpdateFaxSettingDto
{
    public string? Name { get; set; }
    public string? GatewayType { get; set; }
    public string GatewayUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? ApiKey { get; set; }
    public string SenderFaxNumber { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}





