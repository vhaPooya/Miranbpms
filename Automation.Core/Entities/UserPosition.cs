namespace Automation.Core.Entities;

/// <summary>
/// ارتباط کاربر و پست سازمانی
/// </summary>
public class UserPosition : BaseEntity
{
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public int PositionId { get; set; }
    public virtual Position Position { get; set; } = null!;

    public bool IsPrimary { get; set; } = false;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

