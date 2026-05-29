using CORE.Abstract;
using CORE.Concrete.Tenancy;
using CORE.Config;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Extensions;

/// <summary>
///     Multi-tenancy DI. When <c>MultiTenancySettings.IsEnabled = false</c>, registers
///     <see cref="NullTenant" /> so every other consumer of <see cref="ITenant" /> compiles and
///     runs without behavioural change. When enabled, picks <see cref="HeaderTenantResolver" />
///     or <see cref="ClaimsTenantResolver" /> per
///     <see cref="MultiTenancySettings.Provider" />.
/// </summary>
public static class MultiTenancyExtensions
{
    public static IServiceCollection AddMultiTenancy(this IServiceCollection services, ConfigSettings config)
    {
        if (!config.MultiTenancySettings.IsEnabled)
        {
            services.TryAddScoped<ITenant, NullTenant>();
            return services;
        }

        switch (config.MultiTenancySettings.Provider)
        {
            case TenantResolverProvider.Claims:
                services.TryAddScoped<ITenant, ClaimsTenantResolver>();
                break;
            case TenantResolverProvider.Header:
            default:
                services.TryAddScoped<ITenant, HeaderTenantResolver>();
                break;
        }

        return services;
    }
}