namespace Automation.Core.Entities;

/// <summary>
/// جایگاه/پست سازمانی (منابع انسانی)
/// </summary>
public class Position : BaseEntity
{
    /// <summary>
    /// کد پست
    /// </summary>
    public string PositionCode { get; set; } = string.Empty;

    /// <summary>
    /// عنوان پست
    /// </summary>
    public string PositionName { get; set; } = string.Empty;

    /// <summary>
    /// پست والد (سلسله‌مراتب)
    /// </summary>
    public int? ParentPositionId { get; set; }
    public virtual Position? ParentPosition { get; set; }

    /// <summary>
    /// فرزندان پست
    /// </summary>
    public virtual ICollection<Position> ChildPositions { get; set; } = new List<Position>();

    /// <summary>
    /// سازمان
    /// </summary>
    public int? OrganizationId { get; set; }
    public virtual Organization? Organization { get; set; }

    /// <summary>
    /// واحد سازمانی
    /// </summary>
    public int? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// ارتباط با کاربران
    /// </summary>
    public virtual ICollection<UserPosition> UserPositions { get; set; } = new List<UserPosition>();

    /// <summary>
    /// ارتباط با سمت‌ها (Roles)
    /// </summary>
    public virtual ICollection<RolePosition> RolePositions { get; set; } = new List<RolePosition>();
}

