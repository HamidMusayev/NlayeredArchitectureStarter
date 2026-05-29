using CORE.Abstract;
using CORE.Config;
using Microsoft.Extensions.Configuration;

namespace CORE.Concrete.FeatureFlags;

/// <summary>
///     Default <see cref="IFeatureFlagService" /> — reads flags directly from <see cref="IConfiguration" />.
///     Zero infrastructure. Per-user evaluation collapses to the global flag value (the config
///     store doesn't carry targeting metadata) — switch to LaunchDarkly when you need cohorts.
/// </summary>
public sealed class ConfigFeatureFlagService(IConfiguration configuration, ConfigSettings config) : IFeatureFlagService
{
    public Task<bool> IsEnabledAsync(string flag, CancellationToken ct = default)
    {
        var path = $"{config.FeatureFlagSettings.FlagsSection}:{flag}";
        var raw = configuration[path];
        return Task.FromResult(bool.TryParse(raw, out var value) && value);
    }

    public Task<bool> IsEnabledForUserAsync(string flag, string userKey, CancellationToken ct = default)
    {
        return IsEnabledAsync(flag, ct);
    }
}