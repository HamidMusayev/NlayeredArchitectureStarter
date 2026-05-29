using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;

namespace DAL.EntityFramework.Abstract;

public interface IOutboxRepository : IGenericRepository<OutboxMessage>
{
    /// <summary>
    ///     Returns the oldest pending outbox rows (<c>ProcessedOnUtc IS NULL</c>) up to
    ///     <paramref name="batchSize" />, ordered by <c>OccurredOnUtc</c>. Used by the
    ///     dispatcher to drain the table FIFO.
    /// </summary>
    Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default);
}