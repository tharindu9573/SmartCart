using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SmartCart.Core.Interfaces.IServices.Application;

namespace SmartCart.Infrastructure.Services;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        _cache.TryGetValue(key, out string? json);
        if (json == null) return Task.FromResult<T?>(default);
        return Task.FromResult(JsonConvert.DeserializeObject<T>(json));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonConvert.SerializeObject(value);
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromHours(1)
        };
        _cache.Set(key, json, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
        => Task.FromResult(_cache.TryGetValue(key, out _));
}
