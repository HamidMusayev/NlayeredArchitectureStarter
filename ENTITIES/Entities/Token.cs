using ENTITIES.Entities.Generic;

namespace ENTITIES.Entities;

public class Token : Auditable, IEntity
{
    public virtual required User User { get; set; }
    public Guid UserId { get; set; }
    public required string AccessToken { get; set; }
    public DateTimeOffset AccessTokenExpireDate { get; set; }
    public required string RefreshToken { get; set; }
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