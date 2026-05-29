namespace CORE.Abstract;

/// <summary>
///     Vendor-neutral cache surface. Implementations ship under <c>CORE.Concrete.Cache</c>:
///     <c>MemoryCacheService</c> (default, in-process) and <c>RedisCacheService</c>
///     (distributed). Selected via <see cref="CORE.Config.CacheSettings.Provider" />.
///     <para>
///         Values are serialized as JSON. Pass a stable, namespaced <paramref name="key" />
///         (e.g. <c>"users:{id}"</c>) — both impls treat the string as opaque.
///     </para>
/// </summary>
public interface ICacheService
{
    /// <summary>
    ///     Returns the cached value or <c>null</c> if missing/expired.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;

    /// <summary>
    ///     Sets the value with an optional absolute TTL. When <paramref name="ttl" /> is
    ///     <c>null</c> the impl's default applies (Memory: forever-until-evict; Redis: forever).
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default) where T : class;

    /// <summary>
    ///     Removes the key. No-op if missing.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    ///     Read-through helper: returns cached value if present; otherwise calls
    ///     <paramref name="factory" />, caches the result with <paramref name="ttl" />,
    ///     and returns it.
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null, CancellationToken ct = default) where T : class;
}