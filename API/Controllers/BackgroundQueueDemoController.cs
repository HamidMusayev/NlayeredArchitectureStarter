using CORE.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NOTIFICATIONS.Abstract;

namespace API.Controllers;

/// <summary>
///     Demonstration endpoints for <see cref="IBackgroundTaskQueue" />. The captured delegate
///     runs after the HTTP response is already on the wire, inside a fresh
///     <see cref="IServiceScope" /> — so resolve scoped services (EF Core, <c>ICurrentUser</c>,
///     etc.) from the supplied <see cref="IServiceProvider" />, never close over the request's
///     services.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class BackgroundQueueDemoController(IBackgroundTaskQueue queue) : ControllerBase
{
    /// <summary>
    ///     Fire-and-forget email: returns 202 immediately, the actual send happens on the
    ///     background queue consumer.
    /// </summary>
    [HttpPost("send-email")]
    public async Task<IActionResult> SendEmail([FromQuery] string email, [FromQuery] string subject,
        [FromQuery] string body, CancellationToken ct)
    {
        await queue.EnqueueAsync(async (sp, token) =>
        {
            var mail = sp.GetRequiredService<IMailService>();
            await mail.SendAsync(email, subject, body, token);
        }, ct);

        return Accepted(new { note = "Email queued. Delivery happens on the background consumer." });
    }

    /// <summary>
    ///     Simulates a slow side-effect (e.g. cache warm-up, webhook ping) so you can verify the
    ///     response returns before the work completes — check the logs for ordering.
    /// </summary>
    [HttpPost("simulate-work")]
    public async Task<IActionResult> SimulateWork([FromQuery] int delaySeconds = 3, CancellationToken ct = default)
    {
        var enqueuedAt = DateTimeOffset.UtcNow;

        await queue.EnqueueAsync(async (sp, token) =>
        {
            var logger = sp.GetRequiredService<ILogger<BackgroundQueueDemoController>>();
            logger.LogInformation("Background work started, enqueued at {EnqueuedAt}", enqueuedAt);
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), token);
            logger.LogInformation("Background work finished after {Delay}s", delaySeconds);
        }, ct);

        return Accepted(new { enqueuedAt, note = $"Work queued; will complete in ~{delaySeconds}s. Watch the logs." });
    }
}