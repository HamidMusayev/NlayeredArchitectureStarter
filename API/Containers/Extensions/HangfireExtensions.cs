using API.Filters;
using API.HangfireJobs;
using CORE.Config;
using Hangfire;
using Hangfire.PostgreSql;

namespace API.Containers.Extensions;

/// <summary>
///     Registers Hangfire backed by PostgreSQL storage and a server with CPU-proportional worker
///     count. <c>UseHangfireDashboard</c> mounts the dashboard at <c>/api/hangfire</c> (JWT-gated)
///     and registers all recurring jobs so the whole feature lives in one place.
/// </summary>
public static class HangfireExtensions
{
    public static IServiceCollection AddHangfireJobs(this IServiceCollection services, ConfigSettings config)
    {
        services.AddHangfire(configuration =>
            configuration.UsePostgreSqlStorage(
                options => options.UseNpgsqlConnection(config.ConnectionStrings.AppDb),
                new PostgreSqlStorageOptions
                {
                    SchemaName = "hangfire",
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = TimeSpan.FromSeconds(5)
                }));

        services.AddHangfireServer(options => { options.WorkerCount = Math.Max(1, Environment.ProcessorCount / 2); });

        return services;
    }

    public static WebApplication UseHangfireDashboard(this WebApplication app)
    {
        app.UseHangfireDashboard("/api/hangfire", new DashboardOptions
        {
            Authorization = [new HangfireAuthorizationFilter()]
        });

        // Recurring jobs registered here so the whole feature lives in one file.
        RecurringJob.AddOrUpdate<CounterJob>(
            "sample-counter-job",
            job => job.Run(JobCancellationToken.Null),
            "*/30 * * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        return app;
    }
}