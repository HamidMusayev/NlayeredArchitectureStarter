using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.Concrete;

/// <summary>
///     EF Core implementation of <see cref="IOutboxRepository" />. Extends the generic surface
///     with <see cref="IOutboxRepository.GetPendingAsync" /> (FIFO drain for the dispatcher,
///     excluding dead-lettered rows) and <see cref="IOutboxRepository.GetDeadLetteredAsync" />
///     (paged listing for the admin endpoint).
/// </summary>
public class OutboxRepository(DataContext dataContext)
    : GenericRepository<OutboxMessage>(dataContext), IOutboxRepository
{
    private readonly DataContext _dataContext = dataContext;

    public Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default)
    {
        return _dataContext.OutboxMessages
            .Where(o => o.ProcessedOnUtc == null && o.DeadLetteredAt == null)
            .OrderBy(o => o.OccurredOnUtc)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public Task<List<OutboxMessage>> GetDeadLetteredAsync(int skip, int take, CancellationToken ct = default)
    {
        return _dataContext.OutboxMessages
            .Where(o => o.DeadLetteredAt != null)
            .OrderByDescending(o => o.DeadLetteredAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public Task<int> PruneProcessedOlderThanAsync(DateTimeOffset cutoff, CancellationToken ct = default)
    {
        return _dataContext.OutboxMessages
            .Where(o => o.ProcessedOnUtc != null
                        && o.ProcessedOnUtc < cutoff
                        && o.DeadLetteredAt == null)
            .ExecuteDeleteAsync(ct);
    }
}