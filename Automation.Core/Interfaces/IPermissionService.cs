using Automation.Core.Entities;

namespace Automation.Core.Interfaces;

/// <summary>
/// سرویس مدیریت مجوزها
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// بررسی آیا کاربر مجوز دارد؟
    /// </summary>
    Task<bool> HasPermissionAsync(int userId, string permissionCode);
    
    /// <summary>
    /// بررسی آیا کاربر مجوز دارد؟ (با Resource ID)
    /// </summary>
    Task<bool> HasPermissionAsync(int userId, string permissionCode, int? resourceId);
    
    /// <summary>
    /// بررسی آیا کاربر مجوز فرم دارد؟
    /// </summary>
    Task<bool> HasFormPermissionAsync(int userId, int formId, string permissionCode);
    
    /// <summary>
    /// بررسی آیا کاربر مجوز فیلد دارد؟
    /// </summary>
    Task<bool> HasFieldPermissionAsync(int userId, int formId, string fieldName, string permissionCode);
    
    /// <summary>
    /// بررسی آیا کاربر مجوز گردش کار دارد؟
    /// </summary>
    
    /// <summary>
    /// بررسی آیا کاربر می‌تواند به مدرک دسترسی داشته باشد؟
    /// </summary>
    Task<bool> CanAccessDocumentAsync(int userId, int documentId);
    
    /// <summary>
    /// بررسی آیا کاربر می‌تواند مدرک را ویرایش کند؟
    /// </summary>
    Task<bool> CanEditDocumentAsync(int userId, int documentId);
    
    /// <summary>
    /// بررسی آیا مدرک قفل است؟
    /// </summary>
    Task<bool> IsDocumentLockedAsync(int documentId);
    
    /// <summary>
    /// دریافت لیست مجوزهای کاربر
    /// </summary>
    Task<List<string>> GetUserPermissionsAsync(int userId);
    
    /// <summary>
    /// دریافت لیست مجوزهای نقش
    /// </summary>
    Task<List<string>> GetRolePermissionsAsync(int roleId);
    
    /// <summary>
    /// دریافت لیست مجوزهای گروه
    /// </summary>
    Task<List<string>> GetGroupPermissionsAsync(int groupId);
    
    /// <summary>
    /// دریافت Action Type بر اساس کد
    /// </summary>
    Task<ActionType?> GetActionTypeByCodeAsync(string actionCode);
    
    /// <summary>
    /// ایجاد مجوزهای خودکار برای فرم
    /// </summary>
    Task CreateFormPermissionsAsync(int formId, string formName);
    
    /// <summary>
    /// ایجاد مجوزهای خودکار برای گردش کار
    /// </summary>
    
    /// <summary>
    /// ایجاد مجوزهای خودکار برای گزارش
    /// </summary>
    Task CreateReportPermissionsAsync(int reportId, string reportName);
}




