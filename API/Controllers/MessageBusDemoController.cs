using MESSAGEBUS.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OUTBOX.Sample;

namespace API.Controllers;

/// <summary>
///     Demonstration endpoints for <see cref="IMessageBus" />. Publishes a
///     <see cref="UserSignedUpMessage" /> directly through the bus — the registered
///     <see cref="UserSignedUpHandler" /> picks it up on a background worker and logs.
///     <para>
///         Contrast with <c>OutboxDemoController</c>: there the message is parked in an
///         <c>OutboxMessage</c> row inside the same transaction as the domain write, then the
///         dispatcher hands it to this same bus on its next tick. Reach for the bus directly
///         when there is no transactional write to bundle with; reach for the outbox when there
///         is and you need the two to commit atomically.
///     </para>
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class MessageBusDemoController(IMessageBus bus) : ControllerBase
{
    /// <summary>
    ///     Single publish — fire-and-forget. The returned Task completes once the bus has
    ///     accepted the message, not once the handler finishes.
    /// </summary>
    [HttpPost("publish")]
    public async Task<IActionResult> Publish([FromQuery] string email, CancellationToken ct)
    {
        var msg = new UserSignedUpMessage(Guid.NewGuid(), email, DateTimeOffset.UtcNow);
        await bus.PublishAsync(msg, ct);
        return Accepted(new { msg, note = "Published. Watch logs for UserSignedUpHandler output." });
    }

    /// <summary>
    ///     Fans out <paramref name="count" /> messages in one request to show that handlers run
    ///     on background workers — the response returns before all messages are handled.
    /// </summary>
    [HttpPost("publish-batch")]
    public async Task<IActionResult> PublishBatch([FromQuery] int count = 5, CancellationToken ct = default)
    {
        var publishedAt = DateTimeOffset.UtcNow;

        for (var i = 1; i <= count; i++)
            await bus.PublishAsync(
                new UserSignedUpMessage(Guid.NewGuid(), $"batch-user-{i:D3}@example.com", DateTimeOffset.UtcNow),
                ct);

        return Accepted(new
        {
            count,
            publishedAt,
            note = "All messages enqueued. Handler log lines will follow on background workers."
        });
    }
}