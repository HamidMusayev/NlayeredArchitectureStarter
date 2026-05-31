using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.Concrete;

/// <summary>
///     EF Core implementation of <see cref="IAuditLogRepository" />. The prune helper uses
///     <c>ExecuteDeleteAsync</c> for a single bulk DELETE round-trip rather than loading rows
///     into the change tracker.
/// </summary>
public class AuditLogRepository(DataContext dataContext)
    : GenericRepository<AuditLog>(dataContext), IAuditLogRepository
{
    public Task<int> PruneOlderThanAsync(DateTimeOffset cutoff, CancellationToken ct = default)
    {
        return dataContext.AuditLogs
            .Where(a => a.OccurredAt < cutoff)
            .ExecuteDeleteAsync(ct);
    }
}