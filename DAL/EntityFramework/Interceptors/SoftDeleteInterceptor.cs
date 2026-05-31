using ENTITIES.Entities.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DAL.EntityFramework.Interceptors;

/// <summary>
///     Transparently rewrites a <c>Remove()</c> on any <see cref="Auditable" /> entity into a
///     soft delete: flips <see cref="Microsoft.EntityFrameworkCore.EntityState.Deleted" /> →
///     <see cref="Microsoft.EntityFrameworkCore.EntityState.Modified" /> and sets
///     <see cref="Auditable.IsDeleted" /> to true. The downstream
///     <see cref="AuditableInterceptor" /> then stamps <c>DeletedAt</c> / <c>DeletedBy</c>.
///     <para>
///         <b>Order matters:</b> this interceptor MUST be registered before
///         <see cref="AuditableInterceptor" /> so its state-flip is visible when the audit
///         pass runs.
///     </para>
/// </summary>
public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private static void Apply(DbContext? context)
    {
        if (context is null) return;

        foreach (var entry in context.ChangeTracker.Entries()
                     .Where(e => e.State == EntityState.Deleted && e.Entity is Auditable))
        {
            entry.State = EntityState.Modified;
            ((Auditable)entry.Entity).IsDeleted = true;
        }
    }
}