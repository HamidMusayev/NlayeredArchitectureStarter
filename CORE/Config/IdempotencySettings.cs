namespace CORE.Config;

/// <summary>
///     Idempotency-store provider + default TTL. Default is
///     <see cref="IdempotencyProvider.Memory" /> — process-local; switch to
///     <see cref="IdempotencyProvider.Redis" /> for multi-instance deployments so all
///     replicas honour the same keys.
/// </summary>
public record IdempotencySettings
{
    public IdempotencyProvider Provider { get; set; } = IdempotencyProvider.Memory;

    /// <summary>
    ///     How long a key remains "seen" after a successful response. Stripe uses 24h;
    ///     shorter TTLs reduce store footprint but increase the window for the client to
    ///     notice and re-send a stale write.
    /// </summary>
    public int TtlMinutes { get; set; } = 60 * 24;

    /// <summary>
    ///     Header name the middleware reads. Standard is <c>Idempotency-Key</c>.
    /// </summary>
    public string HeaderName { get; set; } = "Idempotency-Key";
}

public enum IdempotencyProvider
{
    Memory = 0,
    Redis = 1
}