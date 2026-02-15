namespace Automation.Core.Entities;

/// <summary>
/// سازمان‌ها
/// </summary>
public class Organization : BaseEntity
{
    /// <summary>
    /// کد سازمان
    /// </summary>
    public string OrganizationCode { get; set; } = string.Empty;
    
    /// <summary>
    /// نام سازمان
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// آیا سازمان مادر/مرجع است
    /// </summary>
    public bool IsParentOrganization { get; set; } = false;

    /// <summary>
    /// آیا سازمان زیرمجموعه/تابعه است
    /// </summary>
    public bool IsSubsidiary { get; set; } = false;

    /// <summary>
    /// بخش‌های سازمان
    /// </summary>
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
    
    /// <summary>
    /// کاربران سازمان
    /// </summary>
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    
    /// <summary>
    /// دبیرخانه‌های سازمان
    /// </summary>
    public virtual ICollection<Secretariat> Secretariats { get; set; } = new List<Secretariat>();
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
}



