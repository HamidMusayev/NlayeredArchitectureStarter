using System.Text.Json;
using CORE.Abstract;
using StackExchange.Redis;

namespace CORE.Concrete.Idempotency;

/// <summary>
///     Distributed <see cref="IIdempotencyStore" /> over Redis. All replicas reach the same
///     keys so a retry that arrives at a different host is still recognized.
/// </summary>
public sealed class RedisIdempotencyStore(IConnectionMultiplexer redis) : IIdempotencyStore
{
    private IDatabase Db => redis.GetDatabase();

    public async Task<IdempotentResponse?> TryGetAsync(string key, CancellationToken ct = default)
    {
        var raw = await Db.StringGetAsync(Key(key));
        return raw.IsNullOrEmpty ? null : JsonSerializer.Deserialize<IdempotentResponse>((string)raw!);
    }

    public Task SetAsync(string key, IdempotentResponse response, TimeSpan ttl, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(response);
        return Db.StringSetAsync(Key(key), json, ttl);
    }

    private static string Key(string key)
    {
        return $"idem:{key}";
    }
}