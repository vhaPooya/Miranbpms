using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ارسال فکس
/// </summary>
public class SendFaxDto
{
    [Required]
    public int SettingsId { get; set; }
    
    [Required]
    public string ToFaxNumber { get; set; } = string.Empty;
    
    public string? ToName { get; set; }
    
    [Required]
    public IFormFile File { get; set; } = null!;
}





