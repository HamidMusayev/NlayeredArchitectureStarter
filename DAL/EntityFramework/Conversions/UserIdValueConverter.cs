using ENTITIES.Identifiers;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DAL.EntityFramework.Conversions;

/// <summary>
///     EF Core converter bridging <see cref="UserId" /> ↔ <c>uuid</c>. Registered globally in
///     <c>DataContext.ConfigureConventions</c>.
/// </summary>
public sealed class UserIdValueConverter : ValueConverter<UserId, Guid>
{
    public UserIdValueConverter()
        : base(id => id.Value, value => new UserId(value))
    {
    }
}