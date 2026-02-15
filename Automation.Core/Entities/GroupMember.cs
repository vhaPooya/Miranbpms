namespace Automation.Core.Entities;

/// <summary>
/// اعضای گروه
/// </summary>
public class GroupMember : BaseEntity
{
    /// <summary>
    /// شناسه گروه
    /// </summary>
    public int GroupId { get; set; }
    public virtual Group Group { get; set; } = null!;
    
    /// <summary>
    /// شناسه کاربر
    /// </summary>
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// تاریخ عضویت
    /// </summary>
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Notes { get; set; }
}



