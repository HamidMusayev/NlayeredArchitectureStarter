using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ENTITIES.Identifiers;

/// <summary>
///     Strongly-typed identifier for a user. Wraps a <see cref="Guid" /> but doesn't implicitly
///     convert to or from one — passing a <c>RoleId</c> where a <see cref="UserId" /> is
///     expected becomes a compile-time error.
///     <para>
///         Storage shape stays <c>uuid</c> in Postgres via the <c>UserIdValueConverter</c>
///         registered globally in <c>DataContext.ConfigureConventions</c>. Wire shape stays
///         <c>Guid</c> via <see cref="UserIdJsonConverter" /> + the <see cref="TypeConverter" />.
///     </para>
///     <para>
///         Used pervasively: every foreign key into <c>User</c> (audit-trail fields on
///         <c>Auditable</c>, <c>Token.UserId</c>, <c>AuditLog.UserId</c>), the
///         <c>ICurrentUser</c> contract, and every service that takes a user id as a parameter.
///     </para>
/// </summary>
[JsonConverter(typeof(UserIdJsonConverter))]
[TypeConverter(typeof(UserIdTypeConverter))]
public readonly record struct UserId(Guid Value)
{
    public static UserId None => new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }
}

public sealed class UserIdJsonConverter : JsonConverter<UserId>
{
    public override UserId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new UserId(reader.GetGuid());
    }

    public override void Write(Utf8JsonWriter writer, UserId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}

public sealed class UserIdTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || sourceType == typeof(Guid) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        return value switch
        {
            string s => new UserId(Guid.Parse(s)),
            Guid g => new UserId(g),
            _ => base.ConvertFrom(context, culture, value)
        };
    }
}