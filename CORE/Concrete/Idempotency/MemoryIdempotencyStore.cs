using CORE.Abstract;
using Microsoft.Extensions.Caching.Memory;

namespace CORE.Concrete.Idempotency;

/// <summary>
///     Process-local <see cref="IIdempotencyStore" /> backed by <see cref="IMemoryCache" />.
///     Suitable for single-instance deployments. Replicas don't share state — flip
///     <c>IdempotencySettings.Provider</c> to <c>Redis</c> when scaling out.
/// </summary>
public sealed class MemoryIdempotencyStore(IMemoryCache cache) : IIdempotencyStore
{
    public Task<IdempotentResponse?> TryGetAsync(string key, CancellationToken ct = default)
    {
        cache.TryGetValue<IdempotentResponse>(Key(key), out var value);
        return Task.FromResult(value);
    }

    public Task SetAsync(string key, IdempotentResponse response, TimeSpan ttl, CancellationToken ct = default)
    {
        cache.Set(Key(key), response, ttl);
        return Task.CompletedTask;
    }

    private static string Key(string key)
    {
        return $"idem:{key}";
    }
}