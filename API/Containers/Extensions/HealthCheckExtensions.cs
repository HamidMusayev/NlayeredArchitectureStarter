using CORE.Config;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace API.Containers.Extensions;

/// <summary>
///     Real ASP.NET Core health checks. <c>/health/live</c> (process alive) and <c>/health/ready</c>
///     (deps probed). Lives next to Nummy's <c>/nummy/health</c> — both reachable.
/// </summary>
public static class HealthCheckExtensions
{
    private const string ReadyTag = "ready";

    public static IServiceCollection AddCoreHealthChecks(this IServiceCollection services, ConfigSettings config)
    {
        var hc = services.AddHealthChecks()
            .AddNpgSql(config.ConnectionStrings.AppDb, name: "postgres", tags: [ReadyTag]);

        if (config.RedisSettings.IsEnabled && !string.IsNullOrWhiteSpace(config.RedisSettings.Connection))
        {
            var redisConn =
                config.RedisSettings.Connection.Replace("redis://", string.Empty, StringComparison.OrdinalIgnoreCase);
            hc.AddRedis(redisConn, "redis", tags: [ReadyTag]);
        }

        if (config.MongoDbSettings.IsEnabled && !string.IsNullOrWhiteSpace(config.MongoDbSettings.Connection))
            hc.AddMongoDb(_ => new MongoClient(config.MongoDbSettings.Connection),
                name: "mongodb",
                tags: [ReadyTag]);

        if (config.ElasticSearchSettings.IsEnabled &&
            !string.IsNullOrWhiteSpace(config.ElasticSearchSettings.Connection))
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