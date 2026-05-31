using ENTITIES.Entities.Generic;
using ENTITIES.Identifiers;

namespace ENTITIES.Entities;

public class Token : Auditable, IEntity
{
    public virtual required User User { get; set; }
    public UserId UserId { get; set; }

    /// <summary>
    ///     JWT <c>jti</c> claim — a per-issuance Guid embedded in the access token. Identifies
    ///     the token row without ever storing the JWT itself; revocation and validation lookups
    ///     all key on this. The full JWT lives only on the wire and in the client's possession.
    /// </summary>
    public required Guid Jti { get; set; }

    public DateTimeOffset AccessTokenExpireDate { get; set; }

    /// <summary>
    ///     SHA-256 hex digest of the refresh token. Indexed and queried by
    ///     <c>TokenRepository.GetByRefreshTokenHashAsync</c> — the plaintext refresh token never
    ///     lands on disk. Clients hold the plaintext (returned once on login via the wire DTO)
    ///     and send it back on rotation; the server hashes it before lookup.
    /// </summary>
    public required string RefreshTokenHash { get; set; }

    public DateTimeOffset RefreshTokenExpireDate { get; set; }

    /// <summary>
    ///     Groups every refresh token derived from the same original login. On rotation the
    ///     new token inherits the parent's family; if anyone re-presents a token whose
    ///     <see cref="UsedAt" /> is non-null, every row in the family is revoked — that pattern
    ///     only happens when the rotated-out token has been stolen.
    /// </summary>
    public Guid FamilyId { get; set; }

    /// <summary>
    ///     UTC timestamp the refresh token was consumed (rotated out). Null while live.
    ///     When non-null the token must never authorize anything — re-presentation is the
    ///     reuse-detection signal that revokes the whole <see cref="FamilyId" />.
    /// </summary>
    public DateTimeOffset? UsedAt { get; set; }

    /// <summary>
    ///     Set when the family is revoked (logout, reuse-detection, admin action). Once true
    ///     nothing in the family validates, regardless of <see cref="UsedAt" />.
    /// </summary>
    public bool IsRevoked { get; set; }
}