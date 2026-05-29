namespace CORE.Abstract;

/// <summary>
///     Cross-process mutual exclusion. Lock handle is disposable — dispose to release.
///     Implementations ship under <c>CORE.Concrete.Locks</c>:
///     <list type="bullet">
///         <item><c>InMemoryDistributedLock</c> — single-process. Loud warning: this is NOT cross-instance.</item>
///         <item>
///             <c>RedisDistributedLock</c> — <c>SET NX PX</c> + Lua release; safe across replicas pointing at the same
///             Redis.
///         </item>
///         <item><c>PostgresAdvisoryLock</c> — uses <c>pg_advisory_lock</c> on the primary DB; no extra infra.</item>
///     </list>
/// </summary>
public interface IDistributedLock
{
    /// <summary>
    ///     Tries to acquire <paramref name="resource" />. Returns a disposable handle on success,
    ///     or <c>null</c> if the lock couldn't be obtained before <paramref name="wait" /> elapsed.
    ///     Always dispose the handle (preferably via <c>await using</c>).
    /// </summary>
    /// <param name="resource">Stable name identifying the lock scope (e.g. <c>"order:42:checkout"</c>).</param>
    /// <param name="wait">Max time to keep retrying. <c>null</c> = single attempt.</param>
    /// <param name="lifetime">Hard upper bound on the held lock — protects against stuck holders. Default 30s.</param>
    Task<ILockHandle?> AcquireAsync(string resource, TimeSpan? wait = null, TimeSpan? lifetime = null,
        CancellationToken ct = default);
}

/// <summary>
///     Owned handle to a distributed lock. Disposing releases.
/// </summary>
public interface ILockHandle : IAsyncDisposable
{
    string Resource { get; }
}