namespace Automation.Core.Entities;

/// <summary>
/// Base entity with common properties
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    
    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// تاریخ ویرایش
    /// </summary>
    public DateTime? EditDate { get; set; }
    
    /// <summary>
    /// شناسه کاربر ایجادکننده
    /// </summary>
    public int? CreatorUserId { get; set; }
    
    /// <summary>
    /// شناسه نقش کاربر ایجادکننده (Legacy - برای سازگاری)
    /// </summary>
    public int? CreatorRoleId { get; set; }
    
    /// <summary>
    /// فعال/غیرفعال
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// حذف شده (Soft Delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    // Legacy properties for backward compatibility
    public DateTime CreatedAt 
    { 
        get => CreationDate; 
        set => CreationDate = value; 
    }
    
    public DateTime? UpdatedAt 
    { 
        get => EditDate; 
        set => EditDate = value; 
    }
}


