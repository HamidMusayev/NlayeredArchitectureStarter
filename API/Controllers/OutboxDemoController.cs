using DAL.EntityFramework.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OUTBOX.Abstract;
using OUTBOX.Sample;

namespace API.Controllers;

/// <summary>
///     Demonstration endpoints for the transactional Outbox pattern. Shows the two canonical
///     shapes:
///     <list type="number">
///         <item>
///             <description>
///                 Enqueue inside an explicit <c>IUnitOfWork.ExecuteInTransactionAsync</c> —
///                 the domain write and the <c>OutboxMessage</c> row commit atomically.
///             </description>
///         </item>
///         <item>
///             <description>
///                 Enqueue + manual <c>CommitAsync</c> — same guarantee, terser when there
///                 is no other domain write to bundle in.
///             </description>
///         </item>
///     </list>
///     The <c>OutboxDispatcherHostedService</c> picks up pending rows on its next tick and hands
///     them to <see cref="MESSAGEBUS.Abstract.IMessageBus" />. <see cref="OUTBOX.Sample.UserSignedUpHandler" />
///     receives the published message and writes a log line — that's the visible side effect.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class OutboxDemoController(IOutbox outbox, IUnitOfWork uow) : ControllerBase
{
    /// <summary>
    ///     Enqueues a <see cref="UserSignedUpMessage" /> inside a transaction. In a real service
    ///     the lambda would also perform the domain write (e.g. <c>userRepo.AddAsync(user)</c>) so
    ///     both the row and the outbox entry commit together or not at all.
    /// </summary>
    [HttpPost("enqueue-in-transaction")]
    public async Task<IActionResult> EnqueueInTransaction([FromQuery] string email, CancellationToken ct)
    {
        var userId = Guid.NewGuid();

        await uow.ExecuteInTransactionAsync(async () =>
        {
            // ... here is where the domain write would happen (userRepo.AddAsync(user), etc.) ...

            await outbox.EnqueueAsync(
                new UserSignedUpMessage(userId, email, DateTimeOffset.UtcNow), ct);

            return userId;
        }, ct);

        return Ok(new
        {
            userId,
            note =
                "OutboxMessage row committed. Dispatcher will publish on next poll tick — watch the logs for UserSignedUpHandler."
        });
    }

    /// <summary>
    ///     Same guarantee, terser shape: enqueue then manually <c>CommitAsync</c>. Use this when
    ///     the endpoint has nothing else to write in the same transaction.
    /// </summary>
    [HttpPost("enqueue-and-commit")]
    public async Task<IActionResult> EnqueueAndCommit([FromQuery] string email, CancellationToken ct)
    {
        var userId = Guid.NewGuid();

        await outbox.EnqueueAsync(
            new UserSignedUpMessage(userId, email, DateTimeOffset.UtcNow), ct);

        await uow.CommitAsync(ct);

        return Ok(new
        {
            userId,
            note =
                "OutboxMessage row committed. Dispatcher will publish on next poll tick — watch the logs for UserSignedUpHandler."
        });
    }
}