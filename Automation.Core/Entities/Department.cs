namespace Automation.Core.Entities;

/// <summary>
/// بخش‌ها
/// </summary>
public class Department : BaseEntity
{
    /// <summary>
    /// کد بخش
    /// </summary>
    public string DepartmentCode { get; set; } = string.Empty;
    
    /// <summary>
    /// نام بخش
    /// </summary>
    public string DepartmentName { get; set; } = string.Empty;
    
    /// <summary>
    /// شناسه سازمان والد
    /// </summary>
    public int? OrganizationId { get; set; }
    public virtual Organization? Organization { get; set; }

    /// <summary>
    /// شناسه واحد والد (برای ساختار سلسله‌مراتبی)
    /// </summary>
    public int? ParentDepartmentId { get; set; }
    public virtual Department? ParentDepartment { get; set; }

    /// <summary>
    /// واحدهای زیرمجموعه
    /// </summary>
    public virtual ICollection<Department> ChildDepartments { get; set; } = new List<Department>();

    /// <summary>
    /// سمت‌های این واحد سازمانی
    /// </summary>
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    /// <summary>
    /// کاربران این بخش
    /// </summary>
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
}



