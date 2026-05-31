using ENTITIES.Identifiers;

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
    public UserId? CreatedById { get; set; }
    public UserId? ModifiedBy { get; set; }
    public UserId? DeletedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    ///     Owning tenant id (strongly-typed wrapper around <see cref="Guid" />). Stamped from
    ///     <see cref="ENTITIES.Identifiers.TenantId" /> on insert by <c>AuditableInterceptor</c>
    ///     and used by <c>DataContext</c>'s global query filter to scope reads to the current
    ///     tenant. Null = "shared / non-tenant" or an unresolved request.
    /// </summary>
    public TenantId? TenantId { get; set; }
}