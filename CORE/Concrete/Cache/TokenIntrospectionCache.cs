using System.Security.Cryptography;
using System.Text;
using CORE.Abstract;
using Microsoft.Extensions.Logging;

namespace CORE.Concrete.Cache;

/// <summary>
///     Default <see cref="ITokenIntrospectionCache" /> backed by the project's vendor-neutral
///     <see cref="ICacheService" /> (Memory or Redis depending on <c>CacheSettings.Provider</c>).
///     <para>
///         Cache values are tiny strings:
///         <list type="bullet">
///             <item><c>"r"</c> — revoked.</item>
///             <item><c>"v:{sha256(refreshToken)}"</c> — valid, with the issued refresh hash inline so the
///                 filter can verify the pair without a second store call.</item>
///         </list>
///         Access tokens are never stored as keys — only their SHA-256 hash. Same for refresh tokens.
///     </para>
///     <para>Backend errors fail-open: reads return <see cref="TokenCacheStatus.Unknown" />, writes log-and-swallow.</para>
/// </summary>
public sealed class TokenIntrospectionCache(ICacheService cache, ILogger<TokenIntrospectionCache> logger)
    : ITokenIntrospectionCache
{
    private const string Revoked = "r";
    private const string ValidPrefix = "v:";

    public async Task<TokenCacheState> GetAsync(string accessToken, CancellationToken ct = default)
    {
        try
        {
            var raw = await cache.GetAsync<string>(Key(accessToken), ct);
            if (raw is null) return new TokenCacheState(TokenCacheStatus.Unknown, null);
            if (raw == Revoked) return new TokenCacheState(TokenCacheStatus.Revoked, null);
            if (raw.StartsWith(ValidPrefix, StringComparison.Ordinal))
                return new TokenCacheState(TokenCacheStatus.Valid, raw[ValidPrefix.Length..]);
            return new TokenCacheState(TokenCacheStatus.Unknown, null);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Token introspection cache GET failed; falling through to DB");
            return new TokenCacheState(TokenCacheStatus.Unknown, null);
        }
    }

    public async Task MarkValidAsync(string accessToken, string refreshToken, TimeSpan ttl,
        CancellationToken ct = default)
    {
        if (ttl <= TimeSpan.Zero) return;
        try
        {
            await cache.SetAsync(Key(accessToken), ValidPrefix + Hash(refreshToken), ttl, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Token introspection cache SET (valid) failed; next read will hit the DB");
        }
    }

    public async Task MarkRevokedAsync(string accessToken, TimeSpan ttl, CancellationToken ct = default)
    {
        if (ttl <= TimeSpan.Zero) return;
        try
        {
            await cache.SetAsync(Key(accessToken), Revoked, ttl, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Token introspection cache SET (revoked) failed; next read will hit the DB");
        }
    }

    /// <summary>SHA-256 hex digest. Public so consumers can pre-hash for comparison.</summary>
    public static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexStringLower(bytes);
    }

    private static string Key(string accessToken) => "tok:" + Hash(accessToken);
}
