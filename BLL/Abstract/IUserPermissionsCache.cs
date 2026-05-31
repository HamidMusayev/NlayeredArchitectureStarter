namespace BLL.Abstract;

/// <summary>
///     Read-through cache for a user's permission <c>Key</c> set. First call hits the DB
///     (user → role → permissions); subsequent calls within TTL return from the cache. Use from
///     an authorization filter / policy handler to keep per-request permission checks sub-ms.
///     <para>
///         Invalidate from any write path that affects what a user can do:
///         <list type="bullet">
///             <item><c>UserService.UpdateAsync</c> — user's role might have changed.</item>
///             <item>
///                 <c>RoleService.UpdateAsync</c> — the role's permission set might have changed (invalidate every user
///                 holding the role).
///             </item>
///             <item><c>RoleService.SoftDeleteAsync</c> — same.</item>
///         </list>
///         When <c>CacheSettings.Provider = Redis</c>, invalidation crosses replicas naturally.
///         When <c>= Memory</c>, each replica's stale entry expires on TTL (default 1 h).
///     </para>
/// </summary>
public interface IUserPermissionsCache
{
    /// <summary>Returns the permission key set for the user, hitting the DB on cache miss.</summary>
    Task<IReadOnlySet<string>> GetAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Drops the cached entry. Safe to call when no entry exists.</summary>
    Task InvalidateAsync(Guid userId, CancellationToken ct = default);
}