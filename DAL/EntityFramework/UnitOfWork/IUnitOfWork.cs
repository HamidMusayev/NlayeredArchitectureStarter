using Microsoft.EntityFrameworkCore.Storage;

namespace DAL.EntityFramework.UnitOfWork;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken ct = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    Task RunInTransactionAsync(Func<Task> work, CancellationToken ct = default);
}