namespace CORE.Config;

/// <summary>
///     Selects the active <c>ICacheService</c> implementation.
///     Default is <see cref="CacheProvider.Memory" /> — single-process, zero external dependency.
///     Switch to <see cref="CacheProvider.Redis" /> for multi-instance deployments — uses the
///     existing <see cref="RedisSettings.Connection" />.
/// </summary>
public record CacheSettings
{
    public CacheProvider Provider { get; set; } = CacheProvider.Memory;

    /// <summary>Default TTL when callers don't specify one. Zero / negative = no expiration.</summary>
    public int DefaultTtlSeconds { get; set; } = 300;

    /// <summary>
    ///     Extra seconds added to <c>ITokenIntrospectionCache</c> entries on top of the JWT's
    ///     remaining lifetime. Keeps revocation markers alive a hair past <c>exp</c> so a clock-
    ///     skewed replica can't briefly honor a token whose marker has already expired.
    /// </summary>
    public int TokenCacheGraceSeconds { get; set; } = 60;
}

public enum CacheProvider
{
    Memory = 0,
    Redis = 1
}