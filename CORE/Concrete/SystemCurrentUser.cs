using CORE.Abstract;
using ENTITIES.Identifiers;

namespace CORE.Concrete;

/// <summary>
///     <see cref="ICurrentUser" /> implementation for non-HTTP execution contexts:
///     Hangfire jobs, hosted services, integration test seeders, CLI tooling.
///     <para>
///         Returns a well-known seed Guid (<see cref="SeedUserId" />) and role <c>"system"</c>
///         instead of <c>null</c>, so <c>Auditable.CreatedBy</c> / <c>UpdatedBy</c> columns are
///         populated and provenance is preserved across both user-driven and background changes.
///     </para>
///     <para>
///         To use inside a background job:
///         <code>
/// using var scope = serviceProvider.CreateScope();
/// var db = scope.ServiceProvider.GetRequiredService&lt;DataContext&gt;();
/// // Replace the scoped HttpCurrentUser with a SystemCurrentUser for this scope.
/// </code>
///         (Tier ADD-6 will add a clean <c>CreateSystemScope()</c> helper.)
///     </para>
/// </summary>
public sealed class SystemCurrentUser : ICurrentUser
{
    /// <summary>
    ///     Well-known seed identifier for the "system" actor. Stable across deployments so
    ///     you can filter / audit system-originated writes consistently.
    /// </summary>
    public static readonly UserId SeedUserId = new(new Guid("00000000-0000-0000-0000-000000000001"));

    public UserId? UserId => SeedUserId;
    public string? Role => "system";
}