using DAL.EntityFramework.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace DAL.EntityFramework.UnitOfWork;

public sealed class UnitOfWork(DataContext context) : IUnitOfWork
{
    public Task<int> CommitAsync(CancellationToken ct = default)
    {
        return context.SaveChangesAsync(ct);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        return context.Database.BeginTransactionAsync(ct);
    }

    public async Task RunInTransactionAsync(Func<Task> work, CancellationToken ct = default)
    {
        await using var tx = await context.Database.BeginTransactionAsync(ct);
        try
        {
            await work();
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}