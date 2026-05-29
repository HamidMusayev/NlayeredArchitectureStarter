using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.Concrete;

/// <summary>
///     EF Core implementation of <see cref="IOutboxRepository" />. Extends the generic surface with
///     <see cref="IOutboxRepository.GetPendingAsync" /> which returns the oldest unprocessed
///     <see cref="OutboxMessage" /> rows in FIFO order for the dispatcher background service.
/// </summary>
public class OutboxRepository(DataContext dataContext)
    : GenericRepository<OutboxMessage>(dataContext), IOutboxRepository
{
    private readonly DataContext _dataContext = dataContext;

    public Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default)
    {
        return _dataContext.OutboxMessages
            .Where(o => o.ProcessedOnUtc == null)
            .OrderBy(o => o.OccurredOnUtc)
            .Take(batchSize)
            .ToListAsync(ct);
    }
}