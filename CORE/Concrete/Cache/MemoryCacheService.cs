using CORE.Abstract;
using CORE.Config;
using Microsoft.Extensions.Caching.Memory;

namespace CORE.Concrete.Cache;

/// <summary>
///     In-process <see cref="ICacheService" /> built on <see cref="IMemoryCache" />. Default
///     implementation — zero infrastructure dependency. Suitable for single-instance deployments
///     and dev/CI; flip to <see cref="RedisCacheService" /> when running multiple instances so
///     invalidations propagate.
/// </summary>
public sealed class MemoryCacheService(IMemoryCache cache, ConfigSettings config) : ICacheService
{
    private TimeSpan? DefaultTtl =>
        config.CacheSettings.DefaultTtlSeconds > 0
            ? TimeSpan.FromSeconds(config.CacheSettings.DefaultTtlSeconds)
            : null;

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default) where T : class
    {
        var options = new MemoryCacheEntryOptions();
        var effective = ttl ?? DefaultTtl;
        if (effective is { } span && span > TimeSpan.Zero)
            options.AbsoluteExpirationRelativeToNow = span;

        cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        cache.Remove(key);
        return Task.CompletedTask;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null, CancellationToken ct = default) where T : class
    {
        if (cache.TryGetValue(key, out T? cached) && cached is not null) return cached;

        var produced = await factory(ct);
        await SetAsync(key, produced, ttl, ct);
        return produced;
    }
}