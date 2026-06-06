using Newtonsoft.Json;
using StackExchange.Redis;
using SmartCart.Core.Interfaces.IServices.Application;

namespace SmartCart.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDatabase _db;

    public CacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (!value.HasValue) return default;
        return JsonConvert.DeserializeObject<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonConvert.SerializeObject(value);
        await _db.StringSetAsync(key, json, expiry ?? TimeSpan.FromHours(1));
    }

    public async Task RemoveAsync(string key) => await _db.KeyDeleteAsync(key);

    public async Task<bool> ExistsAsync(string key) => await _db.KeyExistsAsync(key);
}
