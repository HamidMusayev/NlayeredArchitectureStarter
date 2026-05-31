using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;

namespace DAL.EntityFramework.Abstract;

/// <summary>
///     Append-and-prune repository for <see cref="AuditLog" />. Writes happen via
///     <see cref="IGenericRepository{TEntity}.AddAsync" /> and a commit; reads happen via the
///     standard inherited query methods. The retention job uses <see cref="PruneOlderThanAsync" />
///     to batch-delete rows past the retention window.
/// </summary>
public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    /// <summary>Bulk-deletes audit rows with <c>OccurredAt &lt; <paramref name="cutoff" /></c>. Returns the rows deleted.</summary>
    Task<int> PruneOlderThanAsync(DateTimeOffset cutoff, CancellationToken ct = default);
}