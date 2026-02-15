namespace Automation.Core.Entities;

/// <summary>
/// ارتباط سمت (Role) و پست سازمانی
/// </summary>
public class RolePosition : BaseEntity
{
    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;

    public int PositionId { get; set; }
    public virtual Position Position { get; set; } = null!;
}

