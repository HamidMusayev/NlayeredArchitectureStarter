using API.Attributes;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
///     Operator-facing endpoints for the transactional outbox. Use to inspect rows the
///     dispatcher has given up on (after <c>MessageBusSettings.OutboxMaxAttempts</c> failures)
///     and to resurrect specific ones once the underlying cause is fixed. Authenticated +
///     <c>[ValidateToken]</c>-gated; tighten with an admin role check if your project has one.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateToken]
public class OutboxAdminController(IOutboxRepository outboxRepository, IUnitOfWork unitOfWork) : ControllerBase
{
    /// <summary>Paged list of dead-lettered rows (newest first). Includes the last error for triage.</summary>
    [HttpGet("dead-letter")]
    public async Task<IActionResult> ListDeadLetter([FromQuery] int skip = 0, [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        var rows = await outboxRepository.GetDeadLetteredAsync(Math.Max(0, skip), Math.Clamp(take, 1, 200), ct);
        return Ok(rows.Select(r => new
        {
            r.Id, r.Type, r.OccurredOnUtc, r.AttemptCount, r.LastAttemptedAt, r.DeadLetteredAt, r.Error
        }));
    }

    /// <summary>
    ///     Clears <c>DeadLetteredAt</c> and zeroes <c>AttemptCount</c> on the row so the next
    ///     dispatcher tick picks it up. Use after fixing the underlying cause (deployed handler,
    ///     unblocked downstream). Returns 404 if the row doesn't exist or wasn't dead-lettered.
    /// </summary>
    [HttpPost("dead-letter/{id:guid}/resurrect")]
    public async Task<IActionResult> Resurrect(Guid id, CancellationToken ct)
    {
        var row = await outboxRepository.GetAsync(o => o.Id == id, true);
        if (row is null || row.DeadLetteredAt is null)
            return NotFound(new { id, note = "No dead-lettered row with that id." });

        row.DeadLetteredAt = null;
        row.AttemptCount = 0;
        row.Error = null;
        await unitOfWork.CommitAsync(ct);

        return Ok(new { id, note = "Resurrected. Next dispatcher tick will retry." });
    }
}