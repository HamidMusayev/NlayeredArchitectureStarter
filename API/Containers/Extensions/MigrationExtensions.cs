using CORE.Config;
using DAL.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace API.Containers.Extensions;

/// <summary>
///     Optionally applies pending EF Core migrations at startup. Controlled by
///     <c>MigrationSettings.RunOnStartup</c> — defaults to <c>false</c> in production. When
///     enabled, creates a short-lived DI scope, calls <c>db.Database.Migrate()</c>, and logs the
///     outcome. Throws on failure so the application does not start against a stale schema.
/// </summary>
public static class MigrationExtensions
{
    public static WebApplication ApplyPendingMigrationsIfConfigured(this WebApplication app, ConfigSettings config)
    {
        if (!config.MigrationSettings.RunOnStartup) return app;

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Migrations");

        try
        {
            logger.LogInformation("Applying pending EF Core migrations on startup...");
            db.Database.Migrate();
            logger.LogInformation("Migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Migration on startup failed.");
            throw;
        }

        return app;
    }
}