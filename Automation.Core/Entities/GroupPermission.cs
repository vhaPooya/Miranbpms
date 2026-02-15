namespace Automation.Core.Entities;

/// <summary>
/// مجوزهای گروه
/// </summary>
public class GroupPermission : BaseEntity
{
    /// <summary>
    /// شناسه گروه
    /// </summary>
    public int GroupId { get; set; }
    public virtual Group Group { get; set; } = null!;
    
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



