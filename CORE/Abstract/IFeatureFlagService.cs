namespace CORE.Abstract;

/// <summary>
///     Vendor-neutral feature-flag surface. Implementations live under
///     <c>CORE.Concrete.FeatureFlags</c>: <c>ConfigFeatureFlagService</c> (default — flags are
///     just booleans in <c>IConfiguration</c>) and <c>LaunchDarklyFeatureFlagService</c>
///     (SaaS, targeting). Selected via <see cref="CORE.Config.FeatureFlagSettings.Provider" />.
///     <para>
///         Typical usage:
///         <code>if (await flags.IsEnabledAsync("new-checkout")) ...</code>
///         Pass the current user id (or any stable per-actor key) to
///         <see cref="IsEnabledForUserAsync" /> so percentage rollouts and targeting cohorts
///         resolve consistently across requests.
///     </para>
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    ///     Anonymous evaluation — for global on/off flags or "default-off" rollouts that
    ///     don't depend on who is asking.
    /// </summary>
    Task<bool> IsEnabledAsync(string flag, CancellationToken ct = default);

    /// <summary>
    ///     Per-user evaluation. <paramref name="userKey" /> must be stable for a given actor
    ///     across requests so percentage-rollout buckets stay sticky.
    /// </summary>
    Task<bool> IsEnabledForUserAsync(string flag, string userKey, CancellationToken ct = default);
}