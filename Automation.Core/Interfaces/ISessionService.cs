namespace Automation.Core.Interfaces;

/// <summary>
/// سرویس مدیریت Session
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// دریافت شناسه دبیرخانه جاری از Session
    /// </summary>
    int? GetCurrentSecretariatId();
    
    /// <summary>
    /// تنظیم شناسه دبیرخانه جاری در Session
    /// </summary>
    void SetCurrentSecretariatId(int? secretariatId);
    
    /// <summary>
    /// دریافت شناسه کاربر جاری از Session
    /// </summary>
    int? GetCurrentUserId();
    
    /// <summary>
    /// تنظیم شناسه کاربر جاری در Session
    /// </summary>
    void SetCurrentUserId(int userId);
    
    /// <summary>
    /// دریافت شناسه سمت/پوزیشن جاری از Session — OPosId
    /// </summary>
    int? GetCurrentPositionId();

    /// <summary>
    /// تنظیم شناسه سمت/پوزیشن جاری در Session
    /// </summary>
    void SetCurrentPositionId(int? positionId);

    /// <summary>
    /// پاک کردن Session
    /// </summary>
    void Clear();
}




