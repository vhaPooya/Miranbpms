using StackExchange.Redis;
using System.Text.Json;
using System.Text;

namespace Automation.Infrastructure.Services.Caching;

/// <summary>
/// سرویس کش با Redis برای بهبود عملکرد سیستم
/// </summary>
public class RedisCacheService : Automation.Core.Interfaces.ICacheService, ICacheService
{
    private readonly IDatabase _cache;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer redisConnection, ILogger<RedisCacheService> logger)
    {
        _cache = redisConnection.GetDatabase();
        _logger = logger;
    }

    /// <summary>
    /// دریافت مقدار از کش
    /// </summary>
    public async Task<T> GetAsync<T>(string key)
    {
        try
        {
            var cachedValue = await _cache.StringGetAsync(key);
            if (cachedValue.HasValue)
            {
                var serializedValue = Encoding.UTF8.GetString(cachedValue);
                return JsonSerializer.Deserialize<T>(serializedValue);
            }
            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get cache key: {key}");
            return default(T);
        }
    }

    /// <summary>
    /// تنظیم مقدار در کش
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            var encodedValue = Encoding.UTF8.GetBytes(serializedValue);
            
            await _cache.StringSetAsync(key, encodedValue, expiration ?? TimeSpan.FromMinutes(30));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to set cache key: {key}");
        }
    }

    /// <summary>
    /// حذف مقدار از کش
    /// </summary>
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to remove cache key: {key}");
        }
    }

    /// <summary>
    /// بررسی وجود کلید در کش
    /// </summary>
    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await _cache.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to check existence of cache key: {key}");
            return false;
        }
    }

    /// <summary>
    /// افزایش مقدار عددی در کش
    /// </summary>
    public async Task<long> IncrementAsync(string key, long value = 1, TimeSpan? expiration = null)
    {
        try
        {
            var result = await _cache.StringIncrementAsync(key, value, flags: CommandFlags.HighPriority);
            
            if (expiration.HasValue)
            {
                await _cache.KeyExpireAsync(key, expiration);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to increment cache key: {key}");
            return 0;
        }
    }

    /// <summary>
    /// کاهش مقدار عددی در کش
    /// </summary>
    public async Task<long> DecrementAsync(string key, long value = 1, TimeSpan? expiration = null)
    {
        try
        {
            var result = await _cache.StringDecrementAsync(key, value, flags: CommandFlags.HighPriority);
            
            if (expiration.HasValue)
            {
                await _cache.KeyExpireAsync(key, expiration);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to decrement cache key: {key}");
            return 0;
        }
    }

    /// <summary>
    /// افزودن به لیست در کش
    /// </summary>
    public async Task<long> ListAddAsync<T>(string key, T value)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            return await _cache.ListLeftPushAsync(key, serializedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to add to list in cache key: {key}");
            return 0;
        }
    }

    /// <summary>
    /// دریافت لیست از کش
    /// </summary>
    public async Task<List<T>> ListGetAsync<T>(string key)
    {
        try
        {
            var values = await _cache.ListRangeAsync(key);
            var result = new List<T>();
            
            foreach (var value in values)
            {
                var deserialized = JsonSerializer.Deserialize<T>(value);
                result.Add(deserialized);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get list from cache key: {key}");
            return new List<T>();
        }
    }

    /// <summary>
    /// تنظیم زمان انقضا برای کلید
    /// </summary>
    public async Task<bool> ExpireAsync(string key, TimeSpan expiration)
    {
        try
        {
            return await _cache.KeyExpireAsync(key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to set expiration for cache key: {key}");
            return false;
        }
    }

    /// <summary>
    /// دریافت اطلاعات آماری کش
    /// </summary>
    public async Task<CacheStats> GetCacheStatsAsync()
    {
        try
        {
            var info = await _cache.Multiplexer.GetServer(_cache.Multiplexer.GetEndPoints().First())
                .InfoAsync();
            
            var stats = new CacheStats();
            
            foreach (var section in info)
            {
                if (section.Key == "Stats")
                {
                    foreach (var item in section)
                    {
                        switch (item.Key)
                        {
                            case "total_commands_processed":
                                stats.TotalCommandsProcessed = long.Parse(item.Value);
                                break;
                            case "instantaneous_ops_per_sec":
                                stats.InstantaneousOpsPerSec = long.Parse(item.Value);
                                break;
                            case "total_net_input_bytes":
                                stats.TotalNetInputBytes = long.Parse(item.Value);
                                break;
                            case "total_net_output_bytes":
                                stats.TotalNetOutputBytes = long.Parse(item.Value);
                                break;
                        }
                    }
                }
            }
            
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cache stats");
            return new CacheStats();
        }
    }

    /// <summary>
    /// پاک‌سازی کش
    /// </summary>
    public async Task ClearAsync()
    {
        try
        {
            var endpoints = _cache.Multiplexer.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _cache.Multiplexer.GetServer(endpoint);
                await server.FlushDatabaseAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear cache");
        }
    }

    /// <summary>
    /// دریافت کلیدهای مرتبط با الگو
    /// </summary>
    public async Task<List<string>> GetKeysByPatternAsync(string pattern)
    {
        try
        {
            var endpoints = _cache.Multiplexer.GetEndPoints();
            var server = _cache.Multiplexer.GetServer(endpoints.First());
            var keys = server.Keys(pattern: pattern);
            
            return keys.Select(k => k.ToString()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get keys by pattern: {pattern}");
            return new List<string>();
        }
    }

    /// <summary>
    /// تنظیم کش با TTL
    /// </summary>
    public async Task<bool> SetWithTtlAsync<T>(string key, T value, TimeSpan ttl)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            var encodedValue = Encoding.UTF8.GetBytes(serializedValue);
            
            var transaction = _cache.CreateTransaction();
            transaction.AddCondition(Condition.KeyNotExists(key));
            var setResult = transaction.StringSetAsync(key, encodedValue, ttl);
            
            return await transaction.ExecuteAsync() && await setResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to set cache with TTL for key: {key}");
            return false;
        }
    }

    // Automation.Core.Interfaces.ICacheService (sync)
    T? Automation.Core.Interfaces.ICacheService.Get<T>(string key) where T : class =>
        GetAsync<T>(key).GetAwaiter().GetResult();

    void Automation.Core.Interfaces.ICacheService.Set<T>(string key, T value, TimeSpan? expiration) where T : class =>
        SetAsync(key, value, expiration).GetAwaiter().GetResult();

    void Automation.Core.Interfaces.ICacheService.Remove(string key) =>
        RemoveAsync(key).GetAwaiter().GetResult();

    void Automation.Core.Interfaces.ICacheService.Clear() =>
        ClearAsync().GetAwaiter().GetResult();

    T Automation.Core.Interfaces.ICacheService.GetOrSet<T>(string key, Func<T> factory, TimeSpan? expiration) where T : class
    {
        var existing = GetAsync<T>(key).GetAwaiter().GetResult();
        if (existing != null) return existing;
        var value = factory();
        SetAsync(key, value, expiration).GetAwaiter().GetResult();
        return value;
    }
}

/// <summary>
/// رابط استاندارد برای سرویس‌های کش
/// </summary>
public interface ICacheService
{
    Task<T> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
}

/// <summary>
/// آمار کش
/// </summary>
public class CacheStats
{
    public long TotalCommandsProcessed { get; set; }
    public long InstantaneousOpsPerSec { get; set; }
    public long TotalNetInputBytes { get; set; }
    public long TotalNetOutputBytes { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// مدیریت کش برای سناریوهای خاص
/// </summary>
public class CacheManager
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheManager> _logger;

    public CacheManager(ICacheService cacheService, ILogger<CacheManager> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// کش کردن نتایج جستجو
    /// </summary>
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var cachedValue = await _cacheService.GetAsync<T>(key);
        if (cachedValue != null)
        {
            _logger.LogInformation($"Cache HIT for key: {key}");
            return cachedValue;
        }

        _logger.LogInformation($"Cache MISS for key: {key}");
        var newValue = await factory();
        await _cacheService.SetAsync(key, newValue, expiration);
        return newValue;
    }

    /// <summary>
    /// کش کردن اطلاعات کاربر
    /// </summary>
    public async Task<T> GetUserCacheAsync<T>(int userId, string cacheSuffix, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var key = $"user:{userId}:{cacheSuffix}";
        return await GetOrSetAsync(key, factory, expiration);
    }

    /// <summary>
    /// کش کردن اطلاعات سند
    /// </summary>
    public async Task<T> GetDocumentCacheAsync<T>(int documentId, string cacheSuffix, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var key = $"document:{documentId}:{cacheSuffix}";
        return await GetOrSetAsync(key, factory, expiration);
    }

    /// <summary>
    /// کش کردن اطلاعات فرم
    /// </summary>
    public async Task<T> GetFormCacheAsync<T>(int formId, string cacheSuffix, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var key = $"form:{formId}:{cacheSuffix}";
        return await GetOrSetAsync(key, factory, expiration);
    }

    /// <summary>
    /// حذف کش کاربر
    /// </summary>
    public async Task RemoveUserCacheAsync(int userId, string cacheSuffix = null)
    {
        var pattern = cacheSuffix == null ? $"user:{userId}:*" : $"user:{userId}:{cacheSuffix}";
        await RemoveByPatternAsync(pattern);
    }

    /// <summary>
    /// حذف کش سند
    /// </summary>
    public async Task RemoveDocumentCacheAsync(int documentId, string cacheSuffix = null)
    {
        var pattern = cacheSuffix == null ? $"document:{documentId}:*" : $"document:{documentId}:{cacheSuffix}";
        await RemoveByPatternAsync(pattern);
    }

    /// <summary>
    /// حذف کش فرم
    /// </summary>
    public async Task RemoveFormCacheAsync(int formId, string cacheSuffix = null)
    {
        var pattern = cacheSuffix == null ? $"form:{formId}:*" : $"form:{formId}:{cacheSuffix}";
        await RemoveByPatternAsync(pattern);
    }

    /// <summary>
    /// حذف کش با الگو
    /// </summary>
    private async Task RemoveByPatternAsync(string pattern)
    {
        // در Redis، حذف با الگو نیاز به دسترسی به سرور دارد
        // اینجا فقط یک نمونه پیاده‌سازی ساده ارائه شده است
        _logger.LogInformation($"Removing cache entries by pattern: {pattern}");
    }
}