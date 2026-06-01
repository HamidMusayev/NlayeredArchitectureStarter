using API.Filters;
using API.HangfireJobs;
using CORE.Config;
using Hangfire;
using Hangfire.PostgreSql;

namespace API.Extensions;

/// <summary>
///     Registers Hangfire backed by PostgreSQL storage and a server with CPU-proportional worker
///     count. <c>UseHangfireDashboard</c> mounts the dashboard at <c>/api/hangfire</c> (JWT-gated)
///     and registers all recurring jobs so the whole feature lives in one place.
/// </summary>
public static class HangfireExtensions
{
    public static IServiceCollection AddHangfireJobs(this IServiceCollection services, IConfiguration configuration)
    {
        var connections = configuration.GetConfigSection<ConnectionStrings>();

        services.AddHangfire(cfg =>
            cfg.UsePostgreSqlStorage(
                options => options.UseNpgsqlConnection(connections.AppDb),
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
        var configuration = app.Services.GetRequiredService<IConfiguration>();
        var auditLog = configuration.GetConfigSection<AuditLogSettings>();
        var messageBus = configuration.GetConfigSection<MessageBusSettings>();

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

        // Daily prune of audit rows past retention. Cron 02:30 UTC — off-peak for most regions.
        RecurringJob.AddOrUpdate<AuditLogPruneJob>(
            "audit-log-prune",
            job => job.Run(auditLog.RetentionDays, JobCancellationToken.Null),
            "30 2 * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        // Daily prune of successfully-processed outbox rows. 02:45 UTC — staggered after the
        // audit prune so the two don't compete for the DB lock on a small instance.
        RecurringJob.AddOrUpdate<OutboxCleanupJob>(
            "outbox-cleanup",
            job => job.Run(messageBus.OutboxRetentionDays, JobCancellationToken.Null),
            "45 2 * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        return app;
    }
}