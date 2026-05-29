using ENTITIES.Entities.Redis;
using Redis.OM;

namespace API.Services;

/// <summary>
///     Hosted service that creates the Redis search index for the <see cref="Person" /> document
///     on application startup. Runs once; <c>StopAsync</c> is a no-op. Safe to re-run — Redis.OM's
///     <c>CreateIndexAsync</c> is idempotent when the index already exists.
/// </summary>
public class RedisIndexCreatorService(RedisConnectionProvider provider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await provider.Connection.CreateIndexAsync(typeof(Person));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}