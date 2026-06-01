using CORE.Abstract;
using CORE.Config;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CORE.Concrete.Locks;

/// <summary>
///     Redlock-light distributed lock — single-instance Redis. Uses <c>SET key token NX PX</c>
///     for acquisition and a Lua compare-and-delete script for release so a stale holder can't
///     release a lock that was already expired and re-acquired by someone else.
///     <para>
///         For Redis clusters with high availability requirements, swap to a full Redlock
///         implementation (multiple independent masters). This impl is fine for the common
///         "one primary Redis, lock isn't life-safety-critical" use case.
///     </para>
/// </summary>
public sealed class RedisDistributedLock(IConnectionMultiplexer redis, IOptions<DistributedLockSettings> options)
    : IDistributedLock
{
    private const string ReleaseScript = @"
if redis.call('GET', KEYS[1]) == ARGV[1] then
    return redis.call('DEL', KEYS[1])
else
    return 0
end";

    private readonly DistributedLockSettings _settings = options.Value;

    public async Task<ILockHandle?> AcquireAsync(
        string resource,
        TimeSpan? wait = null,
        TimeSpan? lifetime = null,
        CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        var key = Key(resource);
        var token = Guid.NewGuid().ToString("N");
        var ttl = lifetime ?? TimeSpan.FromSeconds(_settings.DefaultLifetimeSeconds);
        var deadline = DateTime.UtcNow +
                       (wait ?? TimeSpan.FromMilliseconds(_settings.DefaultWaitMilliseconds));

        while (true)
        {
            if (await db.StringSetAsync(key, token, ttl, When.NotExists))
                return new Handle(resource, db, key, token);

            if (DateTime.UtcNow >= deadline) return null;
            ct.ThrowIfCancellationRequested();
            await Task.Delay(50, ct);
        }
    }

    private static string Key(string resource)
    {
        return $"lock:{resource}";
    }

    private sealed class Handle(string resource, IDatabase db, string key, string token) : ILockHandle
    {
        public string Resource => resource;

        public async ValueTask DisposeAsync()
        {
            await db.ScriptEvaluateAsync(ReleaseScript, [key], [token]);
        }
    }
}