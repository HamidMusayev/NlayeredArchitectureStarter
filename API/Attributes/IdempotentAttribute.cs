namespace API.Attributes;

/// <summary>
///     Marks an action (or controller) as idempotent. The <c>IdempotencyMiddleware</c> reads
///     the configured header (default <c>Idempotency-Key</c>) and:
///     - replays the cached response when the key was seen within TTL,
///     - or caches the fresh response after the action runs.
///     Methods without this attribute are passed through unchanged.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class IdempotentAttribute : Attribute;