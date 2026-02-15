namespace Automation.Core.Entities;

/// <summary>
/// رابط سمت - گروه (یک سمت می‌تواند در چندین گروه کاربری قرار گیرد)
/// </summary>
public class RoleGroup : BaseEntity
{
    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;

    public int GroupId { get; set; }
    public virtual Group Group { get; set; } = null!;
}


