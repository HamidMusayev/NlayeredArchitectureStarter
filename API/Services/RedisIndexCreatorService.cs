using ENTITIES.Entities.Redis;
using Redis.OM;

namespace API.Services;

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