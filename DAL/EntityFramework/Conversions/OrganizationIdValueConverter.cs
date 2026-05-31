using ENTITIES.Identifiers;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DAL.EntityFramework.Conversions;

/// <summary>
///     EF Core converter bridging <see cref="OrganizationId" /> ↔ <c>uuid</c>. Registered
///     globally in <c>DataContext.ConfigureConventions</c>.
/// </summary>
public sealed class OrganizationIdValueConverter : ValueConverter<OrganizationId, Guid>
{
    public OrganizationIdValueConverter()
        : base(id => id.Value, value => new OrganizationId(value))
    {
    }
}