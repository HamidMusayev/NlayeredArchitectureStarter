namespace CORE.Abstract;

/// <summary>
///     Resolves the current request's tenant. Implementations live under
///     <c>CORE.Concrete.Tenancy</c>: <c>HeaderTenantResolver</c> (X-Tenant-Id header) and
///     <c>ClaimsTenantResolver</c> (<c>tenant_id</c> JWT claim). When
///     <see cref="CORE.Config.MultiTenancySettings.IsEnabled" /> is <c>false</c>, the
///     <c>NullTenant</c> impl is registered and returns <c>null</c> — derived single-tenant
///     projects behave as if tenancy didn't exist.
/// </summary>
public interface ITenant
{
    /// <summary>
    ///     The current tenant id, or <c>null</c> when tenancy is disabled or unresolved.
    ///     <c>DataContext</c>'s query filter treats <c>null</c> as "see everything" so the
    ///     default is fail-open for single-tenant workloads.
    /// </summary>
    Guid? TenantId { get; }
}