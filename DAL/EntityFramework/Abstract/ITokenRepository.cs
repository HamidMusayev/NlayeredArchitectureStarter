using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;

namespace DAL.EntityFramework.Abstract;

public interface ITokenRepository : IGenericRepository<Token>
{
    /// <summary>
    ///     Validates a JWT id + refresh pair: a row with the given <paramref name="jti" /> and
    ///     refresh token must exist, not be revoked, not be already used, and the access token
    ///     must not be expired. Used by the standard <c>[ValidateToken]</c> attribute on
    ///     protected endpoints.
    /// </summary>
    Task<bool> IsValid(Guid jti, string refreshToken);

    /// <summary>
    ///     Same predicate as <see cref="IsValid" /> but returns the matching <see cref="Token" />
    ///     so callers can read <see cref="Token.AccessTokenExpireDate" /> for cache-TTL math.
    ///     Used by <c>TokenService.CheckValidationAsync</c> to populate the introspection cache.
    /// </summary>
    Task<Token?> GetForValidationAsync(Guid jti, string refreshToken, CancellationToken ct = default);

    /// <summary>
    ///     Returns all <see cref="Token.IsDeleted" />=false rows for a JWT id — used by the
    ///     logout path so soft-deletes hit every row sharing the jti (usually one).
    /// </summary>
    Task<List<Token>> GetActiveTokensByJtiAsync(Guid jti);

    /// <summary>
    ///     Loads the token row keyed by SHA-256 hex digest of the refresh token, eagerly
    ///     including the owning <see cref="User" />. The rotation flow hashes the incoming
    ///     plaintext (over TLS) before calling this — the plaintext never lands on disk.
    /// </summary>
    Task<Token?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct = default);

    /// <summary>
    ///     Marks every <see cref="Token" /> in the given family as revoked and returns the
    ///     affected rows. Triggered by refresh-token reuse detection (likely theft). Callers use
    ///     the returned list to push revocation markers into the introspection cache so other
    ///     replicas see the change without waiting for a DB round-trip.
    /// </summary>
    Task<List<Token>> RevokeFamilyAsync(Guid familyId, CancellationToken ct = default);
}