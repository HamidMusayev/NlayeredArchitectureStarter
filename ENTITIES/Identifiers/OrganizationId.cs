using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ENTITIES.Identifiers;

/// <summary>
///     Strongly-typed identifier for an organization. Wraps a <see cref="Guid" /> but doesn't
///     implicitly convert to or from one — passing a <c>UserId</c> where an
///     <see cref="OrganizationId" /> is expected becomes a compile-time error.
///     <para>
///         Storage shape stays <c>uuid</c> in Postgres via the <c>OrganizationIdValueConverter</c>
///         registered globally in <c>DataContext.ConfigureConventions</c>. Wire shape stays
///         <c>Guid</c> via <see cref="OrganizationIdJsonConverter" /> + the
///         <see cref="TypeConverter" />.
///     </para>
/// </summary>
[JsonConverter(typeof(OrganizationIdJsonConverter))]
[TypeConverter(typeof(OrganizationIdTypeConverter))]
public readonly record struct OrganizationId(Guid Value)
{
    public static OrganizationId None => new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }
}

public sealed class OrganizationIdJsonConverter : JsonConverter<OrganizationId>
{
    public override OrganizationId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new OrganizationId(reader.GetGuid());
    }

    public override void Write(Utf8JsonWriter writer, OrganizationId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}

public sealed class OrganizationIdTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || sourceType == typeof(Guid) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        return value switch
        {
            string s => new OrganizationId(Guid.Parse(s)),
            Guid g => new OrganizationId(g),
            _ => base.ConvertFrom(context, culture, value)
        };
    }
}