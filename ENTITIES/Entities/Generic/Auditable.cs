namespace ENTITIES.Entities.Generic;

/// <summary>
///     Base class for every persisted entity that needs audit + soft-delete + multi-tenancy
///     support. Carries the Id, who/when of create/modify/delete, the soft-delete flag, and
///     the owning <see cref="TenantId" />. <c>DataContext</c>'s global query filter
///     automatically hides rows where <see cref="IsDeleted" /> is true and scopes reads to the
///     current tenant.
/// </summary>
public class Auditable
{
    public Guid Id { get; set; }
    public Guid? CreatedById { get; set; }
    public Guid? ModifiedBy { get; set; }
    public Guid? DeletedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    ///     Owning tenant id. Stamped from <c>ITenant.TenantId</c> on insert and used by
    ///     <c>DataContext</c>'s global query filter to scope reads to the current tenant.
    ///     Null = "shared / non-tenant" or an unresolved request.
    /// </summary>
    public Guid? TenantId { get; set; }
}