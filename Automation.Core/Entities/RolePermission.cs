namespace Automation.Core.Entities;

/// <summary>
/// ارتباط نقش‌ها و مجوزها (Many-to-Many)
/// </summary>
public class RolePermission : BaseEntity
{
    /// <summary>
    /// شناسه نقش
    /// </summary>
    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;
    
    /// <summary>
    /// شناسه مجوز
    /// </summary>
    public int PermissionId { get; set; }
    public virtual Permission Permission { get; set; } = null!;
    
    /// <summary>
    /// آیا مجوز اعطا شده است؟
    /// </summary>
    public bool IsGranted { get; set; } = true;
}



