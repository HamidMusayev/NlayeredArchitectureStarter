using CORE.Abstract;
using CORE.Config;
using LaunchDarkly.Sdk;
using LaunchDarkly.Sdk.Server;
using Microsoft.Extensions.Options;

namespace CORE.Concrete.FeatureFlags;

/// <summary>
///     LaunchDarkly-backed <see cref="IFeatureFlagService" />. Supports targeting, percentage
///     rollouts and kill switches without redeploys.
///     <para>
///         When <see cref="LaunchDarklySettings.Offline" /> is true (the dev/CI default) the client
///         is initialized in offline mode — every call returns the configured
///         <see cref="LaunchDarklySettings.DefaultValue" />. Production deployments set
///         <c>Offline = false</c> and supply a real <see cref="LaunchDarklySettings.SdkKey" />.
///     </para>
/// </summary>
public sealed class LaunchDarklyFeatureFlagService : IFeatureFlagService, IDisposable
{
    private readonly LdClient _client;
    private readonly bool _defaultValue;

    public LaunchDarklyFeatureFlagService(IOptions<FeatureFlagSettings> options)
    {
        var settings = options.Value.LaunchDarkly;
        _defaultValue = settings.DefaultValue;

        var ldConfig = Configuration.Builder(settings.SdkKey)
            .Offline(settings.Offline || string.IsNullOrWhiteSpace(settings.SdkKey))
            .Build();

        _client = new LdClient(ldConfig);
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    public Task<bool> IsEnabledAsync(string flag, CancellationToken ct = default)
    {
        return Task.FromResult(_client.BoolVariation(flag, Context.New("anonymous"), _defaultValue));
    }

    public Task<bool> IsEnabledForUserAsync(string flag, string userKey, CancellationToken ct = default)
    {
        return Task.FromResult(_client.BoolVariation(flag, Context.New(userKey), _defaultValue));
    }
}