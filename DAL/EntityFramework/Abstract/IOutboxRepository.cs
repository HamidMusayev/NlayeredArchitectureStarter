using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;

namespace DAL.EntityFramework.Abstract;

public interface IOutboxRepository : IGenericRepository<OutboxMessage>
{
    /// <summary>
    ///     Returns the oldest pending outbox rows (<c>ProcessedOnUtc IS NULL</c> AND
    ///     <c>DeadLetteredAt IS NULL</c>) up to <paramref name="batchSize" />, ordered by
    ///     <c>OccurredOnUtc</c>. Used by the dispatcher to drain the table FIFO. Dead-lettered
    ///     rows are excluded so a poison message can't block the queue head.
    /// </summary>
    Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default);

    /// <summary>
    ///     Paged listing of dead-lettered rows (newest dead-letter first). Used by the admin
    ///     endpoint to inspect and triage poison messages.
    /// </summary>
    Task<List<OutboxMessage>> GetDeadLetteredAsync(int skip, int take, CancellationToken ct = default);

    /// <summary>
    ///     Bulk-deletes successfully-processed rows with <c>ProcessedOnUtc &lt; cutoff</c>.
    ///     Dead-lettered rows are <b>not</b> touched — they need human review. Returns the
    ///     number of rows deleted.
    /// </summary>
    Task<int> PruneProcessedOlderThanAsync(DateTimeOffset cutoff, CancellationToken ct = default);
}