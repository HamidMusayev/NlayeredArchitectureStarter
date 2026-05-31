using CORE.Abstract;
using CORE.Concrete.Tenancy;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Extensions;

/// <summary>
///     Registers <see cref="ClaimsTenantResolver" /> as the project's <see cref="ITenant" />.
///     Always-on — if a derived project is single-tenant, delete this file, the call to
///     <c>AddMultiTenancy</c> in <c>Program.cs</c>, and the <c>TenantId</c> column on
///     <c>Auditable</c>.
/// </summary>
public static class MultiTenancyExtensions
{
    public static IServiceCollection AddMultiTenancy(this IServiceCollection services)
    {
        services.TryAddScoped<ITenant, ClaimsTenantResolver>();
        return services;
    }
}