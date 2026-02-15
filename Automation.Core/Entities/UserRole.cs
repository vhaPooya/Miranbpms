namespace Automation.Core.Entities;

/// <summary>
/// ارتباط کاربران و نقش‌ها (Many-to-Many)
/// </summary>
public class UserRole : BaseEntity
{
    /// <summary>
    /// شناسه کاربر
    /// </summary>
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// شناسه نقش
    /// </summary>
    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;

    /// <summary>
    /// تاریخ اختصاص نقش
    /// </summary>
    public DateTime? AssignedDate { get; set; }
}



