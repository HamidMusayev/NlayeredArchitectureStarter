using API.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
///     Demonstration endpoints for the <see cref="IdempotentAttribute" /> +
///     <c>IdempotencyMiddleware</c> pipeline. Send the <c>Idempotency-Key</c> header (name is
///     configurable in <c>IdempotencySettings.HeaderName</c>) and the middleware will replay the
///     first response for any repeat of that key within TTL — even if the inner action would
///     have produced different output. Omit the header and every call runs fresh.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class IdempotencyDemoController : ControllerBase
{
    /// <summary>
    ///     Returns a fresh server time + random number on every call when no
    ///     <c>Idempotency-Key</c> header is sent. Same header value replays the original body.
    ///     Try it: <c>curl -X POST .../charge -H "Idempotency-Key: abc-123"</c> twice and notice
    ///     the response is byte-identical the second time.
    /// </summary>
    [HttpPost("charge")]
    [Idempotent]
    public IActionResult Charge([FromBody] ChargeRequest body)
    {
        // Imagine this is "create a Stripe charge" — the kind of write you must not double-fire.
        var chargeId = Guid.NewGuid();
        return Ok(new
        {
            chargeId,
            body.Amount,
            body.Currency,
            chargedAt = DateTimeOffset.UtcNow,
            randomServerEcho = Random.Shared.Next(),
            note = "Re-send with the same Idempotency-Key header to get this exact response back."
        });
    }

    /// <summary>
    ///     Same shape, but without <see cref="IdempotentAttribute" /> — every call produces a
    ///     fresh response regardless of the header. Use this side-by-side with <c>charge</c> to
    ///     see what the attribute actually buys you.
    /// </summary>
    [HttpPost("charge-not-idempotent")]
    public IActionResult ChargeNotIdempotent([FromBody] ChargeRequest body)
    {
        var chargeId = Guid.NewGuid();
        return Ok(new
        {
            chargeId,
            body.Amount,
            body.Currency,
            chargedAt = DateTimeOffset.UtcNow,
            randomServerEcho = Random.Shared.Next(),
            note = "No [Idempotent] attribute — repeats produce new responses."
        });
    }

    public sealed record ChargeRequest(decimal Amount, string Currency);
}
