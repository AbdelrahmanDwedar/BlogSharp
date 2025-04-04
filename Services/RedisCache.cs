using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace BlogSharp.Services;

public class RedisCache : ICache
{
    private readonly IDistributedCache _distributedCache;

    public RedisCache(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var cachedData = await _distributedCache.GetStringAsync(key);
        return cachedData != null ? JsonSerializer.Deserialize<T>(cachedData) : default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
    {
        var serializedData = JsonSerializer.Serialize(value);
        await _distributedCache.SetStringAsync(
            key,
            serializedData,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration }
        );
    }

    public async Task RemoveAsync(string key)
    {
        await _distributedCache.RemoveAsync(key);
    }
}
