namespace CORE.Abstract;

/// <summary>
///     Persistence surface for idempotency keys. When a request marked
///     <c>[Idempotent]</c> arrives, the middleware checks this store; if the key has been
///     seen within TTL, the cached response is replayed and the action body is skipped.
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>
    ///     Returns the cached response for <paramref name="key" />, or null if missing/expired.
    /// </summary>
    Task<IdempotentResponse?> TryGetAsync(string key, CancellationToken ct = default);

    /// <summary>
    ///     Stores the response for <paramref name="key" /> with the given <paramref name="ttl" />.
    /// </summary>
    Task SetAsync(string key, IdempotentResponse response, TimeSpan ttl, CancellationToken ct = default);
}

/// <summary>
///     Captured response payload — status code, content-type, body bytes — that can be replayed
///     verbatim when the same idempotency key reappears.
/// </summary>
public sealed record IdempotentResponse(int StatusCode, string? ContentType, byte[] Body);