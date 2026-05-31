using ENTITIES.Identifiers;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DAL.EntityFramework.Conversions;

/// <summary>
///     EF Core <see cref="ValueConverter{TModel,TProvider}" /> that bridges the strongly-typed
///     <see cref="RoleId" /> wrapper to a plain Postgres <c>uuid</c>. Wired globally via
///     <c>DataContext.ConfigureConventions</c> so every property of type <c>RoleId</c>
///     picks it up — no per-entity <c>HasConversion(...)</c> needed.
/// </summary>
public sealed class RoleIdValueConverter : ValueConverter<RoleId, Guid>
{
    public RoleIdValueConverter()
        : base(id => id.Value, value => new RoleId(value))
    {
    }
}