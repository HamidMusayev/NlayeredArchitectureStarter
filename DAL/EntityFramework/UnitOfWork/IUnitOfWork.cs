using Microsoft.EntityFrameworkCore.Storage;

namespace DAL.EntityFramework.UnitOfWork;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken ct = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>
    ///     Runs <paramref name="work" /> inside a single EF transaction. Commits on success;
    ///     rolls back and rethrows on any exception. Use when the caller has no return value.
    /// </summary>
    Task RunInTransactionAsync(Func<Task> work, CancellationToken ct = default);

    /// <summary>
    ///     Runs <paramref name="work" /> inside a single EF transaction and returns its result.
    ///     Commits on success; rolls back and rethrows on any exception. Use to deduplicate
    ///     the try/commit/rollback ceremony in service methods that need a return value.
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> work, CancellationToken ct = default);
}