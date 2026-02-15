namespace Automation.Core.Interfaces;

/// <summary>
/// سرویس Cache برای داده‌های ثابت
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// دریافت مقدار از Cache
    /// </summary>
    T? Get<T>(string key) where T : class;
    
    /// <summary>
    /// ذخیره مقدار در Cache
    /// </summary>
    void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    
    /// <summary>
    /// حذف از Cache
    /// </summary>
    void Remove(string key);
    
    /// <summary>
    /// پاک کردن تمام Cache
    /// </summary>
    void Clear();
    
    /// <summary>
    /// دریافت یا ایجاد مقدار (از Cache یا Factory)
    /// </summary>
    T GetOrSet<T>(string key, Func<T> factory, TimeSpan? expiration = null) where T : class;
}




