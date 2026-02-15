namespace Automation.Core.Entities;

/// <summary>
/// وضعیت‌ها
/// </summary>
public class Status : BaseEntity
{
    /// <summary>
    /// کد وضعیت
    /// </summary>
    public string StatusCode { get; set; } = string.Empty;
    
    /// <summary>
    /// نام وضعیت
    /// </summary>
    public string StatusName { get; set; } = string.Empty;
    
    /// <summary>
    /// رنگ وضعیت
    /// </summary>
    public string? Color { get; set; }
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
}



