using API.Services;
using CORE.Abstract;
using CORE.Concrete.Cache;
using CORE.Config;
using DAL.Redis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Redis.OM;
using StackExchange.Redis;

namespace API.Extensions;

/// <summary>
///     All cache-flavoured registrations: ASP.NET output cache, the <see cref="ICacheService" />
///     abstraction (Memory or Redis), and — when <c>RedisSettings.IsEnabled</c> — the
///     <see cref="Redis.OM.RedisConnectionProvider" />, the index-creator hosted service, and
///     the <c>IPersonRepository</c> example.
/// </summary>
public static class CachingExtensions
{
    public static IServiceCollection AddCaching(this IServiceCollection services, ConfigSettings config)
    {
        services.AddOutputCache(options =>
            options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(2))));

        services.AddMemoryCache();

        // Redis.OM example registration — the in-house Person sample lives here.
        if (config.RedisSettings.IsEnabled)
        {
            services.TryAddSingleton(new RedisConnectionProvider(config.RedisSettings.Connection));
            services.TryAddScoped<IPersonRepository, PersonRepository>();
            services.AddHostedService<RedisIndexCreatorService>();
        }

        // Vendor-neutral ICacheService — switchable via CacheSettings.Provider.
        switch (config.CacheSettings.Provider)
        {
            case CacheProvider.Redis:
                services.TryAddSingleton<IConnectionMultiplexer>(_ =>
                    ConnectionMultiplexer.Connect(
                        config.RedisSettings.Connection.Replace("redis://", string.Empty,
                            StringComparison.OrdinalIgnoreCase)));
                services.TryAddSingleton<ICacheService, RedisCacheService>();
                break;
            case CacheProvider.Memory:
            default:
                services.TryAddSingleton<ICacheService, MemoryCacheService>();
                break;
        }

        return services;
    }

    public static WebApplication UseOutputCachePipeline(this WebApplication app)
    {
        app.UseOutputCache();
        return app;
    }
}