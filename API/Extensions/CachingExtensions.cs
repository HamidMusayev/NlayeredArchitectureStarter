using CORE.Abstract;
using CORE.Concrete.Cache;
using CORE.Config;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace API.Extensions;

/// <summary>
///     Cache-flavoured registrations: ASP.NET output cache, in-memory cache, and the
///     vendor-neutral <see cref="ICacheService" /> abstraction (Memory or Redis, switchable via
///     <c>CacheSettings.Provider</c>). The Redis.OM Person sample lives in
///     <see cref="RedisOmExtensions" /> — keep these two concerns separate.
/// </summary>
public static class CachingExtensions
{
    public static IServiceCollection AddCaching(this IServiceCollection services, ConfigSettings config)
    {
        services.AddOutputCache(options =>
            options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(2))));

        services.AddMemoryCache();

        // Token introspection cache — fronts the Tokens table for per-request validation.
        services.TryAddSingleton<ITokenIntrospectionCache, TokenIntrospectionCache>();

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
