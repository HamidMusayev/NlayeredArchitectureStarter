using CORE.Abstract;
using CORE.Concrete.FeatureFlags;
using CORE.Config;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Extensions;

/// <summary>
///     Registers the <see cref="CORE.Abstract.IFeatureFlagService" /> implementation chosen by
///     <c>FeatureFlagSettings.Provider</c>: <c>Config</c> (default, reads from
///     <see cref="Microsoft.Extensions.Configuration.IConfiguration" />) or <c>LaunchDarkly</c>.
/// </summary>
public static class FeatureFlagsExtensions
{
    public static IServiceCollection AddFeatureFlags(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetConfigSection<FeatureFlagSettings>();

        switch (settings.Provider)
        {
            case FeatureFlagProvider.LaunchDarkly:
                services.TryAddSingleton<IFeatureFlagService, LaunchDarklyFeatureFlagService>();
                break;
            case FeatureFlagProvider.Config:
            default:
                services.TryAddSingleton<IFeatureFlagService, ConfigFeatureFlagService>();
                break;
        }

        return services;
    }
}