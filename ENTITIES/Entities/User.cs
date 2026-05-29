using ENTITIES.Entities.Generic;

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
    public Guid? RoleId { get; set; }
    public virtual Role? Role { get; set; }
    public Guid? ProfileFileId { get; set; }
    public virtual File? ProfileFile { get; set; }
}