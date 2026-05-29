using CORE.Abstract;
using CORE.Concrete.Locks;
using CORE.Config;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace API.Containers.Extensions;

/// <summary>
///     Registers the <see cref="CORE.Abstract.IDistributedLock" /> implementation chosen by
///     <c>DistributedLockSettings.Provider</c>: <c>InMemory</c> (default, single-instance only),
///     <c>Redis</c> (RedLock.net via StackExchange.Redis), or <c>PostgresAdvisory</c>.
/// </summary>
public static class DistributedLockExtensions
{
    public static IServiceCollection AddDistributedLock(this IServiceCollection services, ConfigSettings config)
    {
        switch (config.DistributedLockSettings.Provider)
        {
            case DistributedLockProvider.Redis:
                services.TryAddSingleton<IConnectionMultiplexer>(_ =>
                    ConnectionMultiplexer.Connect(
                        config.RedisSettings.Connection.Replace("redis://", string.Empty,
                            StringComparison.OrdinalIgnoreCase)));
                services.TryAddSingleton<IDistributedLock, RedisDistributedLock>();
                break;
            case DistributedLockProvider.PostgresAdvisory:
                services.TryAddSingleton<IDistributedLock, PostgresAdvisoryLock>();
                break;
            case DistributedLockProvider.InMemory:
            default:
                services.TryAddSingleton<IDistributedLock, InMemoryDistributedLock>();
                break;
        }

        return services;
    }
}