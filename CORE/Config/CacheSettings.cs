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
}

public enum CacheProvider
{
    Memory = 0,
    Redis = 1
}