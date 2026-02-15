namespace Automation.Core.Entities;

/// <summary>
/// دسته‌بندی مجوزها (برای نمایش Tree)
/// </summary>
public class PermissionGroup : BaseEntity
{
    /// <summary>
    /// کد دسته
    /// </summary>
    public string GroupCode { get; set; } = string.Empty;
    
    /// <summary>
    /// نام دسته
    /// </summary>
    public string GroupName { get; set; } = string.Empty;
    
    /// <summary>
    /// شناسه دسته والد (برای ساختار درختی)
    /// </summary>
    public int? ParentGroupId { get; set; }
    public virtual PermissionGroup? ParentGroup { get; set; }
    
    /// <summary>
    /// دسته‌های زیرمجموعه
    /// </summary>
    public virtual ICollection<PermissionGroup> ChildGroups { get; set; } = new List<PermissionGroup>();
    
    /// <summary>
    /// مجوزهای این دسته
    /// </summary>
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    
    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// آیکون (کلاس Bootstrap Icon)
    /// </summary>
    public string? Icon { get; set; }
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
}



