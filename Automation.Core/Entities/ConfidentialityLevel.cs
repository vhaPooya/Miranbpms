namespace Automation.Core.Entities;

/// <summary>
/// سطوح محرمانگی
/// </summary>
public class ConfidentialityLevel : BaseEntity
{
    /// <summary>
    /// کد سطح محرمانگی
    /// </summary>
    public string LevelCode { get; set; } = string.Empty;
    
    /// <summary>
    /// نام سطح محرمانگی
    /// </summary>
    public string LevelName { get; set; } = string.Empty;
    
    /// <summary>
    /// سطح محرمانگی (عدد)
    /// </summary>
    public int Level { get; set; }
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
}



