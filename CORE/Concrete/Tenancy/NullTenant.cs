using CORE.Abstract;

namespace CORE.Concrete.Tenancy;

/// <summary>
///     Default <see cref="ITenant" /> used when <c>MultiTenancySettings.IsEnabled = false</c>.
///     Always returns <c>null</c>, which <c>DataContext</c>'s query filter treats as "see every
///     row" — so a derived single-tenant project behaves as if tenancy didn't exist.
/// </summary>
public sealed class NullTenant : ITenant
{
    public Guid? TenantId => null;
}