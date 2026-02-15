namespace Automation.Core.Entities;

/// <summary>
/// اولویت‌ها
/// </summary>
public class Priority : BaseEntity
{
    /// <summary>
    /// کد اولویت
    /// </summary>
    public string PriorityCode { get; set; } = string.Empty;
    
    /// <summary>
    /// نام اولویت
    /// </summary>
    public string PriorityName { get; set; } = string.Empty;
    
    /// <summary>
    /// سطح اولویت (عدد)
    /// </summary>
    public int PriorityLevel { get; set; }
    
    /// <summary>
    /// رنگ اولویت
    /// </summary>
    public string? Color { get; set; }
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
}



