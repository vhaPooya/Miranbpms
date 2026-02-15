namespace Automation.Core.Entities;

/// <summary>
/// نقش‌های سیستم
/// </summary>
public class Role : BaseEntity
{
    /// <summary>
    /// کد نقش
    /// </summary>
    public string RoleCode { get; set; } = string.Empty;
    
    /// <summary>
    /// نام نقش
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
    
    /// <summary>
    /// شناسه نقش والد (برای ساختار سلسله‌مراتبی)
    /// </summary>
    public int? ParentRoleId { get; set; }
    public virtual Role? ParentRole { get; set; }
    
    /// <summary>
    /// نقش‌های زیرمجموعه
    /// </summary>
    public virtual ICollection<Role> ChildRoles { get; set; } = new List<Role>();
    
    /// <summary>
    /// کاربران دارای این نقش
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    
    /// <summary>
    /// مجوزهای نقش
    /// </summary>
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    /// <summary>
    /// سازمان متصل به سمت (Legacy)
    /// </summary>
    public int? OrganizationId { get; set; }
    public virtual Organization? Organization { get; set; }

    /// <summary>
    /// گروه های کاربری متصل به این سمت
    /// </summary>
    public virtual ICollection<RoleGroup> RoleGroups { get; set; } = new List<RoleGroup>();

    /// <summary>
    /// ?????? ?? ??????
    /// </summary>
    public virtual ICollection<RolePosition> RolePositions { get; set; } = new List<RolePosition>();

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// عنوان مکاتباتی سمت
    /// </summary>
    public string? CorrespondenceTitle { get; set; }

    /// <summary>
    /// عنوان نمایشی سمت
    /// </summary>
    public string? DisplayTitle { get; set; }

    /// <summary>
    /// شناسه پرسنل (کاربر متصل به سمت)
    /// </summary>
    public int? PersonnelUserId { get; set; }
    public virtual User? PersonnelUser { get; set; }

    /// <summary>
    /// شناسه واحد سازمانی
    /// </summary>
    public int? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }
}




