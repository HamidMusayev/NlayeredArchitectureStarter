using System.ComponentModel.DataAnnotations;
using ENTITIES.Entities.Generic;
using ENTITIES.Identifiers;

namespace ENTITIES.Entities;

/// <summary>
///     Tree-structured organizational unit — company, department, branch. Self-referencing
///     via <see cref="ParentId" /> so derived projects can model arbitrarily deep hierarchies.
/// </summary>
public class Organization : Auditable, IEntity
{
    public required string FullName { get; set; }
    public required string ShortName { get; set; }
    public required string Address { get; set; }
    public virtual Organization? Parent { get; set; }
    public OrganizationId? ParentId { get; set; }
    [Phone] public required string PhoneNumber { get; set; }
    [StringLength(10)] public required string Tin { get; set; }
    [EmailAddress] public required string Email { get; set; }
    public required string Rekvizit { get; set; }
    public Guid? LogoFileId { get; set; }
    public virtual File? LogoFile { get; set; }
}