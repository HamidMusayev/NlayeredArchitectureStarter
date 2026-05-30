namespace CORE.Abstract;

/// <summary>
///     Distributed cache fronting the relational <c>Tokens</c> table for the per-request
///     <c>[ValidateToken]</c> check. Three states per access token:
///     <list type="bullet">
///         <item><see cref="TokenCacheStatus.Valid" /> — issued, not revoked, refresh hash present for pair comparison.</item>
///         <item><see cref="TokenCacheStatus.Revoked" /> — explicitly invalidated (logout, family revoke).</item>
///         <item><see cref="TokenCacheStatus.Unknown" /> — cache miss, fall through to the DB and repopulate.</item>
///     </list>
///     <para>
///         Implementations MUST swallow transient cache backend failures and return
///         <see cref="TokenCacheStatus.Unknown" /> so the read path falls through to the database
///         (fail-open). Write paths should log-and-swallow — a missed write means at worst one
///         extra DB hit on the next read.
///     </para>
/// </summary>
public interface ITokenIntrospectionCache
{
    /// <summary>Probes the cache. Never throws — backend failures surface as <see cref="TokenCacheStatus.Unknown" />.</summary>
    Task<TokenCacheState> GetAsync(string accessToken, CancellationToken ct = default);

    /// <summary>Marks the pair valid for <paramref name="ttl" />. The refresh token is stored as a hash for pair comparison.</summary>
    Task MarkValidAsync(string accessToken, string refreshToken, TimeSpan ttl, CancellationToken ct = default);

    /// <summary>Marks the access token revoked for <paramref name="ttl" /> (typically the JWT's remaining lifetime + grace).</summary>
    Task MarkRevokedAsync(string accessToken, TimeSpan ttl, CancellationToken ct = default);
}

/// <summary>Result of a cache probe — never null, always one of the three documented states.</summary>
public readonly record struct TokenCacheState(TokenCacheStatus Status, string? RefreshHash);

public enum TokenCacheStatus
{
    Unknown = 0,
    Valid = 1,
    Revoked = 2
}
