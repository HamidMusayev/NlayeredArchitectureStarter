using API.HealthChecks;
using CORE.Config;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace API.Extensions;

/// <summary>
///     ASP.NET Core health checks. Two HTTP endpoints:
///     <list type="bullet">
///         <item><c>/health/live</c> — process alive (no dependency probes).</item>
///         <item><c>/health/ready</c> — every dependency reachable AND accepting writes.</item>
///     </list>
///     <para>
///         Read-only probes (<c>AddNpgSql</c>, <c>AddRedis</c>) only run a connectivity check —
///         they pass against a primary that's been failed-over to a read-only replica. The
///         custom <see cref="PostgresWriteHealthCheck" /> + <see cref="RedisWriteHealthCheck" />
///         additions perform tiny throw-away writes so failover scenarios surface as unhealthy.
///     </para>
/// </summary>
public static class HealthCheckExtensions
{
    private const string ReadyTag = "ready";

    public static IServiceCollection AddCoreHealthChecks(this IServiceCollection services, ConfigSettings config)
    {
        var hc = services.AddHealthChecks()
            .AddNpgSql(config.ConnectionStrings.AppDb, name: "postgres", tags: [ReadyTag])
            .AddCheck<PostgresWriteHealthCheck>("postgres-write", tags: [ReadyTag]);

        if (!string.IsNullOrWhiteSpace(config.RedisSettings.Connection))
        {
            var redisConn =
                config.RedisSettings.Connection.Replace("redis://", string.Empty, StringComparison.OrdinalIgnoreCase);
            hc.AddRedis(redisConn, "redis", tags: [ReadyTag]);
            hc.AddCheck<RedisWriteHealthCheck>("redis-write", tags: [ReadyTag]);
        }

        if (!string.IsNullOrWhiteSpace(config.MongoDbSettings.Connection))
            hc.AddMongoDb(_ => new MongoClient(config.MongoDbSettings.Connection),
                name: "mongodb",
                tags: [ReadyTag]);

        if (!string.IsNullOrWhiteSpace(config.ElasticSearchSettings.Connection))
            hc.AddElasticsearch(config.ElasticSearchSettings.Connection, "elasticsearch", tags: [ReadyTag]);

        return services;
    }

    public static WebApplication MapCoreHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains(ReadyTag),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }
}