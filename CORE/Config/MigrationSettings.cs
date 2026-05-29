namespace CORE.Config;

/// <summary>
///     Controls automatic EF Core migration on application startup.
///     <para>
///         When <see cref="RunOnStartup" /> is <c>true</c>, <c>db.Database.Migrate()</c> runs
///         before the host starts listening for requests. Convenient for local/dev/CI.
///         Disable in production where migrations should be a deliberate, observable step
///         rather than an implicit side effect of deployment.
///     </para>
/// </summary>
public record MigrationSettings
{
    public bool RunOnStartup { get; set; }
}