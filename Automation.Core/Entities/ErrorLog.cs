namespace Automation.Core.Entities;

/// <summary>
/// لاگ خطاهای سیستم
/// </summary>
public class ErrorLog : BaseEntity
{
    public int? UserId { get; set; }
    public string? ExceptionType { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? Context { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
