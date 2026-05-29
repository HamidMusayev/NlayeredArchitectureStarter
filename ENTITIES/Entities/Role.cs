using ENTITIES.Entities.Generic;

namespace ENTITIES.Entities;

/// <summary>
///     Authorization role — collection of <see cref="Permission" />s assigned to a
///     <see cref="User" /> via <c>User.RoleId</c>. <see cref="Key" /> is the stable lookup
///     identifier; <see cref="Name" /> is the human-readable label.
/// </summary>
public class Role : Auditable, IEntity
{
    public required string Name { get; set; }
    public required string Key { get; set; }
    public virtual List<Permission> Permissions { get; set; } = new();
}