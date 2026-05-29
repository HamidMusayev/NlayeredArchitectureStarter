using ENTITIES.Entities.Generic;

namespace ENTITIES.Entities;

/// <summary>
///     Fine-grained capability (<c>"users.create"</c>, <c>"reports.view"</c>, etc.) attachable
///     to one or more <see cref="Role" />s. Checked by authorization filters; <see cref="Key" />
///     is the stable identifier the code matches against.
/// </summary>
public class Permission : Auditable, IEntity
{
    public required string Name { get; set; }
    public required string Key { get; set; }
    public virtual List<Role> Roles { get; set; } = new();
}