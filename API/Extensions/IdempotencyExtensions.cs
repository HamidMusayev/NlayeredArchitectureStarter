using API.Middlewares;
using CORE.Abstract;
using CORE.Concrete.Idempotency;
using CORE.Config;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace API.Extensions;

/// <summary>
///     Registers the <see cref="CORE.Abstract.IIdempotencyStore" /> implementation chosen by
///     <c>IdempotencySettings.Provider</c> (<c>Memory</c> default or <c>Redis</c>) and wires the
///     <see cref="API.Middlewares.IdempotencyMiddleware" /> into the pipeline via
///     <c>UseIdempotency</c>.
/// </summary>
public static class IdempotencyExtensions
{
    public static IServiceCollection AddIdempotency(this IServiceCollection services, ConfigSettings config)
    {
        switch (config.IdempotencySettings.Provider)
        {
            case IdempotencyProvider.Redis:
                services.TryAddSingleton<IConnectionMultiplexer>(_ =>
                    ConnectionMultiplexer.Connect(
                        config.RedisSettings.Connection.Replace("redis://", string.Empty,
                            StringComparison.OrdinalIgnoreCase)));
                services.TryAddSingleton<IIdempotencyStore, RedisIdempotencyStore>();
                break;
            case IdempotencyProvider.Memory:
            default:
                services.TryAddSingleton<IIdempotencyStore, MemoryIdempotencyStore>();
                break;
        }

        return services;
    }

    public static WebApplication UseIdempotency(this WebApplication app)
    {
        app.UseMiddleware<IdempotencyMiddleware>();
        return app;
    }
}