namespace Automation.Core.Interfaces;

/// <summary>
/// سرویس برای دریافت اطلاعات کاربر جاری
/// </summary>
public interface IUserContextService
{
    /// <summary>
    /// دریافت شناسه کاربر جاری — OUserId
    /// </summary>
    int? GetCurrentUserId();

    /// <summary>
    /// دریافت شناسه سمت/پوزیشن جاری — OPosId
    /// </summary>
    int? GetCurrentPositionId();
    
    /// <summary>
    /// دریافت شناسه سازمان کاربر جاری
    /// </summary>
    Task<int?> GetCurrentUserOrganizationIdAsync();
    
    /// <summary>
    /// دریافت شناسه دپارتمان کاربر جاری
    /// </summary>
    Task<int?> GetCurrentUserDepartmentIdAsync();
}




