using CORE.Abstract;
using CORE.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CORE.Concrete.FeatureFlags;

/// <summary>
///     Default <see cref="IFeatureFlagService" /> — reads flags directly from <see cref="IConfiguration" />.
///     Zero infrastructure. Per-user evaluation collapses to the global flag value (the config
///     store doesn't carry targeting metadata) — switch to LaunchDarkly when you need cohorts.
/// </summary>
public sealed class ConfigFeatureFlagService(IConfiguration configuration, IOptions<FeatureFlagSettings> options)
    : IFeatureFlagService
{
    private readonly FeatureFlagSettings _settings = options.Value;

    public Task<bool> IsEnabledAsync(string flag, CancellationToken ct = default)
    {
        var path = $"{_settings.FlagsSection}:{flag}";
        var raw = configuration[path];
        return Task.FromResult(bool.TryParse(raw, out var value) && value);
    }

    public Task<bool> IsEnabledForUserAsync(string flag, string userKey, CancellationToken ct = default)
    {
        return IsEnabledAsync(flag, ct);
    }
}