namespace CORE.Config;

/// <summary>
///     Selects the active <c>IDistributedLock</c> implementation. Default is
///     <see cref="DistributedLockProvider.InMemory" /> — <b>single-process only</b>.
///     Replace with <see cref="DistributedLockProvider.Redis" /> or
///     <see cref="DistributedLockProvider.PostgresAdvisory" /> the moment you run more than
///     one replica, or you risk concurrent execution of code you assumed was serialized.
/// </summary>
public record DistributedLockSettings
{
    public DistributedLockProvider Provider { get; set; } = DistributedLockProvider.InMemory;

    /// <summary>Default lifetime (TTL) used when callers don't pass one.</summary>
    public int DefaultLifetimeSeconds { get; set; } = 30;

    /// <summary>Default <c>wait</c> when callers don't pass one. Zero = single attempt.</summary>
    public int DefaultWaitMilliseconds { get; set; } = 0;
}

public enum DistributedLockProvider
{
    InMemory = 0,
    Redis = 1,
    PostgresAdvisory = 2
}