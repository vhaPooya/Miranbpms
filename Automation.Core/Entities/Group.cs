namespace Automation.Core.Entities;

/// <summary>
/// گروه‌های کاربری
/// </summary>
public class Group : BaseEntity
{
    /// <summary>
    /// کد گروه
    /// </summary>
    public string GroupCode { get; set; } = string.Empty;
    
    /// <summary>
    /// نام گروه
    /// </summary>
    public string GroupName { get; set; } = string.Empty;
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// شناسه گروه والد (سلسله‌مراتب)
    /// </summary>
    public int? ParentGroupId { get; set; }
    public virtual Group? ParentGroup { get; set; }

    /// <summary>
    /// گروه های زیرمجموعه
    /// </summary>
    public virtual ICollection<Group> ChildGroups { get; set; } = new List<Group>();

    /// <summary>
    /// استفاده در ارجاعات نامه
    /// </summary>
    public bool UseInReferral { get; set; } = false;

    /// <summary>
    /// اعضای گروه
    /// </summary>
    public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    
    /// <summary>
    /// مجوزهای گروه
    /// </summary>
    public virtual ICollection<GroupPermission> GroupPermissions { get; set; } = new List<GroupPermission>();

    /// <summary>
    /// سمت های متصل به این گروه
    /// </summary>
    public virtual ICollection<RoleGroup> RoleGroups { get; set; } = new List<RoleGroup>();
}



