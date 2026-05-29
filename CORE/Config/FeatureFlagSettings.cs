namespace CORE.Config;

/// <summary>
///     Selects the active <c>IFeatureFlagService</c> implementation. Default is
///     <see cref="FeatureFlagProvider.Config" /> — flags live as booleans under the
///     <see cref="FlagsSection" /> in <c>appsettings</c>. Switch to
///     <see cref="FeatureFlagProvider.LaunchDarkly" /> when you need targeting / percentage
///     rollouts / kill switches without redeploying.
/// </summary>
public record FeatureFlagSettings
{
    public FeatureFlagProvider Provider { get; set; } = FeatureFlagProvider.Config;

    /// <summary>
    ///     Config section that the <c>ConfigFeatureFlagService</c> reads. The IConfiguration
    ///     shape is <c>{ "FeatureFlags": { "Flags": { "my-flag": true } } }</c>.
    /// </summary>
    public string FlagsSection { get; set; } = "FeatureFlags:Flags";

    public LaunchDarklySettings LaunchDarkly { get; set; } = new();
}

public enum FeatureFlagProvider
{
    Config = 0,
    LaunchDarkly = 1
}

public record LaunchDarklySettings
{
    public string SdkKey { get; set; } = string.Empty;

    /// <summary>
    ///     Offline = treat every evaluation as <see cref="DefaultValue" />.
    ///     Useful when the SDK key isn't configured in dev/CI.
    /// </summary>
    public bool Offline { get; set; } = true;

    /// <summary>Value returned when offline or on evaluation error.</summary>
    public bool DefaultValue { get; set; }
}