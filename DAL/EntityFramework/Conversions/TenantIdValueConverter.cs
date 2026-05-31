using ENTITIES.Identifiers;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DAL.EntityFramework.Conversions;

/// <summary>
///     EF Core <see cref="ValueConverter{TModel,TProvider}" /> that bridges the strongly-typed
///     <see cref="TenantId" /> wrapper to a plain Postgres <c>uuid</c>. Wired globally via
///     <c>DataContext.ConfigureConventions</c> so every property of type <c>TenantId</c>
///     picks it up — no per-entity <c>HasConversion(...)</c> needed.
///     <para>
///         Lives in <c>DAL</c> (not <c>ENTITIES</c>) so the ENTITIES project stays free of
///         persistence concerns. The wrapper itself ships from <c>ENTITIES.Identifiers</c>.
///     </para>
/// </summary>
public sealed class TenantIdValueConverter : ValueConverter<TenantId, Guid>
{
    public TenantIdValueConverter()
        : base(id => id.Value, value => new TenantId(value))
    {
    }
}