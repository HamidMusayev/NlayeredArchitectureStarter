namespace CORE.Abstract;

/// <summary>
///     Resolves the current request's tenant. The default implementation in
///     <c>CORE.Concrete.Tenancy</c> is <c>ClaimsTenantResolver</c> — reads the
///     <c>MultiTenancySettings.ClaimName</c> JWT claim.
/// </summary>
public interface ITenant
{
    /// <summary>
    ///     The current tenant id, or <c>null</c> when no tenant could be resolved (e.g. an
    ///     anonymous request). <c>DataContext</c>'s query filter treats <c>null</c> as
    ///     "see everything", so the default is fail-open.
    /// </summary>
    Guid? TenantId { get; }
}
