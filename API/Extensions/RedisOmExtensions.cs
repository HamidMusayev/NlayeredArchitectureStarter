using API.Services;
using CORE.Config;
using DAL.Redis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Redis.OM;

namespace API.Extensions;

/// <summary>
///     Redis.OM sample wiring: the <see cref="RedisConnectionProvider" />, the
///     <c>IPersonRepository</c> example, and the <see cref="RedisIndexCreatorService" /> hosted
///     service that builds the indexes on startup. Always-on — if a project does not need the
///     Person sample, delete this file, the call to <c>AddRedisOm</c> in <c>Program.cs</c>, the
///     <c>DAL/Redis</c> folder, and the <see cref="API.Services.PersonRepository" /> sample.
/// </summary>
public static class RedisOmExtensions
{
    public static IServiceCollection AddRedisOm(this IServiceCollection services, ConfigSettings config)
    {
        services.TryAddSingleton(new RedisConnectionProvider(config.RedisSettings.Connection));
        services.TryAddScoped<IPersonRepository, PersonRepository>();
        services.AddHostedService<RedisIndexCreatorService>();
        return services;
    }
}
