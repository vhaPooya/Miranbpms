using System.ComponentModel.DataAnnotations;

namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ایجاد تنظیمات ایمیل
/// </summary>
public class CreateEmailSettingDto
{
    public string? Name { get; set; }
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
    public bool IsDefault { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}





