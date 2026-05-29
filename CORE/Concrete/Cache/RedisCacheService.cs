using System.Text.Json;
using CORE.Abstract;
using CORE.Config;
using StackExchange.Redis;

namespace CORE.Concrete.Cache;

/// <summary>
///     Distributed <see cref="ICacheService" /> over Redis (StackExchange.Redis). Selected when
///     <see cref="CacheSettings.Provider" /> is <see cref="CacheProvider.Redis" /> — the
///     connection string lives in the existing <see cref="RedisSettings.Connection" /> slot.
///     Values are JSON-encoded; key strings are passed through opaquely.
/// </summary>
public sealed class RedisCacheService(IConnectionMultiplexer redis, ConfigSettings config) : ICacheService
{
    private IDatabase Db => redis.GetDatabase();

    private TimeSpan? DefaultTtl =>
        config.CacheSettings.DefaultTtlSeconds > 0
            ? TimeSpan.FromSeconds(config.CacheSettings.DefaultTtlSeconds)
            : null;

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        var raw = await Db.StringGetAsync(key);
        return raw.IsNullOrEmpty ? null : JsonSerializer.Deserialize<T>((string)raw!);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default) where T : class
    {
        var json = JsonSerializer.Serialize(value);
        return Db.StringSetAsync(key, json, ttl ?? DefaultTtl);
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        return Db.KeyDeleteAsync(key);
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null, CancellationToken ct = default) where T : class
    {
        var existing = await GetAsync<T>(key, ct);
        if (existing is not null) return existing;

        var produced = await factory(ct);
        await SetAsync(key, produced, ttl, ct);
        return produced;
    }
}