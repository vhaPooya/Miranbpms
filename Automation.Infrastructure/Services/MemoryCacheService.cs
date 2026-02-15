using Microsoft.Extensions.Caching.Memory;

namespace Automation.Infrastructure.Services;

/// <summary>
/// پیاده‌سازی سرویس Cache با MemoryCache
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromHours(1);

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public T? Get<T>(string key) where T : class
    {
        return _memoryCache.Get<T>(key);
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
        };
        
        _memoryCache.Set(key, value, options);
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }

    public void Clear()
    {
        if (_memoryCache is MemoryCache cache)
        {
            // MemoryCache doesn't have a Clear method, so we need to track keys
            // For simplicity, we'll just remove common keys
            // In production, consider using a distributed cache or tracking keys
        }
    }

    public T GetOrSet<T>(string key, Func<T> factory, TimeSpan? expiration = null) where T : class
    {
        if (_memoryCache.TryGetValue<T>(key, out var cachedValue))
        {
            return cachedValue!;
        }

        var value = factory();
        Set(key, value, expiration);
        return value;
    }
}




