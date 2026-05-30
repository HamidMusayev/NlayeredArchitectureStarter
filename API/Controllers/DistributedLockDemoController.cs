using CORE.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
///     Demonstration endpoints for <see cref="IDistributedLock" />. Hit the same endpoint twice
///     in parallel (two browser tabs, or two <c>curl</c> calls) and watch one win the lock while
///     the other either waits or bails. Behaviour is identical whether the backing provider is
///     InMemory, Redis, or PostgresAdvisory — that's the point of the abstraction.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class DistributedLockDemoController(IDistributedLock locks, ILogger<DistributedLockDemoController> logger)
    : ControllerBase
{
    /// <summary>
    ///     Single-attempt acquire: returns 409 immediately if another caller holds the lock.
    ///     Pattern for "skip if already running" jobs (cache rebuild, daily digest, etc.).
    /// </summary>
    [HttpPost("try-once/{resource}")]
    public async Task<IActionResult> TryOnce(string resource, [FromQuery] int workSeconds = 5,
        CancellationToken ct = default)
    {
        await using var handle = await locks.AcquireAsync(resource, wait: null,
            lifetime: TimeSpan.FromSeconds(workSeconds + 5), ct);

        if (handle is null)
        {
            logger.LogInformation("Lock {Resource} busy — bailing", resource);
            return Conflict(new { resource, note = "Another caller holds the lock. Try again later." });
        }

        logger.LogInformation("Lock {Resource} acquired — working for {Seconds}s", resource, workSeconds);
        await Task.Delay(TimeSpan.FromSeconds(workSeconds), ct);
        logger.LogInformation("Lock {Resource} releasing", resource);

        return Ok(new { resource, note = $"Did the work under exclusive lock for {workSeconds}s." });
    }

    /// <summary>
    ///     Bounded-wait acquire: queues up to <paramref name="waitSeconds" /> waiting for the
    ///     current holder to release. Pattern for "must happen exactly once, but I'm willing to
    ///     wait" — e.g. a checkout flow where the second request should serialize behind the first
    ///     instead of erroring out.
    /// </summary>
    [HttpPost("wait/{resource}")]
    public async Task<IActionResult> Wait(string resource, [FromQuery] int waitSeconds = 10,
        [FromQuery] int workSeconds = 3, CancellationToken ct = default)
    {
        var requestedAt = DateTimeOffset.UtcNow;

        await using var handle = await locks.AcquireAsync(resource,
            wait: TimeSpan.FromSeconds(waitSeconds),
            lifetime: TimeSpan.FromSeconds(workSeconds + 5),
            ct);

        if (handle is null)
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { resource, note = $"Could not acquire within {waitSeconds}s. Try again later." });

        var acquiredAt = DateTimeOffset.UtcNow;
        await Task.Delay(TimeSpan.FromSeconds(workSeconds), ct);

        return Ok(new
        {
            resource,
            requestedAt,
            acquiredAt,
            waitedMs = (acquiredAt - requestedAt).TotalMilliseconds,
            note = "Lock acquired after waiting; work completed."
        });
    }

    /// <summary>
    ///     The canonical "guard a recurring job" shape — what a Hangfire tick on a multi-node
    ///     deployment should look like so only one node actually runs the job each tick.
    /// </summary>
    [HttpPost("recurring-job-guard")]
    public async Task<IActionResult> RecurringJobGuard(CancellationToken ct)
    {
        await using var handle = await locks.AcquireAsync("daily-digest:send",
            wait: null,
            lifetime: TimeSpan.FromMinutes(2),
            ct);

        if (handle is null)
            return Ok(new { ran = false, note = "Another instance already running this tick — skipped." });

        // ... real job body would go here ...
        await Task.Delay(500, ct);

        return Ok(new { ran = true, note = "This instance won the lock and ran the job." });
    }
}
