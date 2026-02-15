namespace Automation.Core.Entities;

/// <summary>
/// شمارنده ترتیبی برای کلیدها
/// </summary>
public class SequenceNumber : BaseEntity
{
    public string SequenceKey { get; set; } = string.Empty;
    public int CurrentValue { get; set; }
}
