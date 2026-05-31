using CORE.Abstract;
using ENTITIES.Entities.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DAL.EntityFramework.Interceptors;

/// <summary>
///     Stamps audit fields (<c>CreatedAt</c>/<c>CreatedById</c>,
///     <c>ModifiedAt</c>/<c>ModifiedBy</c>, <c>DeletedAt</c>/<c>DeletedBy</c>) and the per-row
///     <c>TenantId</c> on <see cref="Auditable" /> entities right before <c>SaveChanges</c>.
///     Replaces the manual <c>SetAuditProperties</c> body that used to live in
///     <c>DataContext.SaveChangesAsync</c>.
///     <para>
///         Pairs with <see cref="SoftDeleteInterceptor" /> — that one runs first and rewrites
///         <c>Remove()</c> as <c>IsDeleted = true</c>; this one then sees the entity as
///         <see cref="EntityState.Modified" /> with <see cref="Auditable.IsDeleted" /> set, and
///         stamps the deletion audit fields accordingly.
///     </para>
/// </summary>
public sealed class AuditableInterceptor(ICurrentUser currentUser, ITenant tenant) : SaveChangesInterceptor
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

    private void Apply(DbContext? context)
    {
        if (context is null) return;

        var now = DateTimeOffset.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries()
                     .Where(e => e.Entity is Auditable &&
                                 e.State is EntityState.Added or EntityState.Modified))
        {
            var entity = (Auditable)entry.Entity;

            switch (entry.State)
            {
                case EntityState.Added:
                    entity.CreatedAt = now;
                    entity.CreatedById = currentUser.UserId;
                    // Stamp tenant when the entity didn't already specify one.
                    if (entity.TenantId is null)
                        entity.TenantId = tenant.TenantId;
                    break;

                case EntityState.Modified:
                    // CreatedAt / CreatedById / TenantId are immutable post-insert.
                    entry.Property(nameof(Auditable.CreatedAt)).IsModified = false;
                    entry.Property(nameof(Auditable.CreatedById)).IsModified = false;
                    entry.Property(nameof(Auditable.TenantId)).IsModified = false;

                    if (entity.IsDeleted)
                    {
                        // Suppress the Modified-* writes — a soft delete shouldn't masquerade
                        // as a regular update.
                        entry.Property(nameof(Auditable.ModifiedBy)).IsModified = false;
                        entry.Property(nameof(Auditable.ModifiedAt)).IsModified = false;

                        entity.DeletedAt = now;
                        entity.DeletedBy = currentUser.UserId;
                    }
                    else
                    {
                        entity.ModifiedAt = now;
                        entity.ModifiedBy = currentUser.UserId;
                    }

                    break;
            }
        }
    }
}