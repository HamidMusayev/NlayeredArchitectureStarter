using System.Linq.Expressions;
using ENTITIES.Entities.Generic;
using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.Context;

/// <summary>
///     Legacy helper that applied a single global query filter (e.g. soft-delete) to every
///     <see cref="Auditable" /> derivative. Superseded by <c>DataContext</c>'s composite
///     soft-delete + tenant filter — kept for backwards compatibility / other contexts.
/// </summary>
public static class ContextExtensions
{
    public static void AddGlobalFilter(this ModelBuilder modelBuilder, string property, object value)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            if (typeof(Auditable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "p");
                var deletedCheck = Expression.Lambda(
                    Expression.Equal(
                        Expression.Property(parameter, property),
                        Expression.Constant(value)
                    ), parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(deletedCheck);
            }
    }
}