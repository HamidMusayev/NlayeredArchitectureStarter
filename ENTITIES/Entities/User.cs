using ENTITIES.Entities.Generic;
using ENTITIES.Identifiers;

namespace ENTITIES.Entities;

/// <summary>
///     Application user — login identity, hashed credentials, optional role + profile picture.
///     Soft-deleted via <see cref="Auditable.IsDeleted" />.
/// </summary>
public class User : Auditable, IEntity
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string ContactNumber { get; set; }
    public required string Password { get; set; }
    public required string Salt { get; set; }
    public string? LastVerificationCode { get; set; }

    /// <summary>
    ///     Running count of consecutive failed login attempts. Reset to 0 on any successful
    ///     login. Crosses <c>AuthSettings.MaxFailedLoginAttempts</c> → <see cref="LockedUntil" />
    ///     is set and the account refuses logins until that timestamp passes.
    /// </summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    ///     UTC moment the lockout expires. Null = no lockout in effect. Compared on every login;
    ///     a still-locked account always returns a generic "invalid credentials" error so the
    ///     lockout state isn't leaked to enumeration probes.
    /// </summary>
    public DateTimeOffset? LockedUntil { get; set; }

    public RoleId? RoleId { get; set; }
    public virtual Role? Role { get; set; }
    public Guid? ProfileFileId { get; set; }
    public virtual File? ProfileFile { get; set; }
}