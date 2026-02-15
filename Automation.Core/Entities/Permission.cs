namespace Automation.Core.Entities;

/// <summary>
/// مجوزها و دسترسی‌ها
/// </summary>
public class Permission : BaseEntity
{
    /// <summary>
    /// کد مجوز
    /// </summary>
    public string PermissionCode { get; set; } = string.Empty;
    
    /// <summary>
    /// نام مجوز
    /// </summary>
    public string PermissionName { get; set; } = string.Empty;
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// دسته‌بندی مجوز (برای سازگاری با نسخه قدیم)
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// شناسه دسته مجوز (برای ساختار درختی)
    /// </summary>
    public int? PermissionGroupId { get; set; }
    public virtual PermissionGroup? PermissionGroup { get; set; }
    
    /// <summary>
    /// ترتیب نمایش در دسته
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// نقش‌های دارای این مجوز
    /// </summary>
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    
    /// <summary>
    /// گروه‌های دارای این مجوز
    /// </summary>
    public virtual ICollection<GroupPermission> GroupPermissions { get; set; } = new List<GroupPermission>();
}



