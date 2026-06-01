using CORE.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
///     Demonstration endpoints for <see cref="IFeatureFlagService" />. The default
///     <c>Config</c> provider reads booleans out of the <c>FeatureFlags:Flags</c> section in
///     <c>appsettings.*.json</c> (e.g. the seeded <c>sample-flag</c>). Swap
///     <c>FeatureFlagSettings.Provider</c> to <c>LaunchDarkly</c> and the same calls hit the
///     SaaS without touching this controller.
///     <para>
///         Try it: hit <c>/api/FeatureFlagDemo/sample-flag</c>, flip
///         <c>FeatureFlags:Flags:sample-flag</c> in <c>appsettings.Development.json</c>, restart,
///         hit it again.
///     </para>
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class FeatureFlagDemoController(IFeatureFlagService flags) : ControllerBase
{
    /// <summary>
    ///     Anonymous evaluation — for global on/off flags or "default-off" rollouts that don't
    ///     depend on who is asking. Returns <c>{ flag, enabled }</c>.
    /// </summary>
    [HttpGet("{flag}")]
    public async Task<IActionResult> IsEnabled(string flag, CancellationToken ct)
    {
        var enabled = await flags.IsEnabledAsync(flag, ct);
        return Ok(new { flag, enabled });
    }

    /// <summary>
    ///     Per-user evaluation. Pass any stable per-actor key (user id, tenant id, session id)
    ///     so percentage-rollout buckets stay sticky across requests for the same actor.
    /// </summary>
    [HttpGet("{flag}/for-user")]
    public async Task<IActionResult> IsEnabledForUser(string flag, [FromQuery] string userKey,
        CancellationToken ct)
    {
        var enabled = await flags.IsEnabledForUserAsync(flag, userKey, ct);
        return Ok(new { flag, userKey, enabled });
    }

    /// <summary>
    ///     The canonical branch shape — call this twice with the seeded <c>sample-flag</c> set
    ///     to <c>true</c> vs <c>false</c> to see the response body change.
    /// </summary>
    [HttpGet("new-checkout-banner")]
    public async Task<IActionResult> NewCheckoutBanner(CancellationToken ct)
    {
        if (await flags.IsEnabledAsync("new-checkout", ct))
            return Ok(new { variant = "new", message = "Try our redesigned checkout!" });

        return Ok(new { variant = "classic", message = "Standard checkout flow." });
    }
}