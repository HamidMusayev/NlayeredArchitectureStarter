using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ENTITIES.Identifiers;

/// <summary>
///     Strongly-typed identifier for a role. Wraps a <see cref="Guid" /> but doesn't implicitly
///     convert to or from one — passing a <c>UserId</c> where a <c>RoleId</c> is expected
///     becomes a compile-time error instead of a silent bug.
///     <para>
///         Storage shape stays <c>uuid</c> in Postgres via the <c>RoleIdValueConverter</c>
///         registered globally in <c>DataContext.ConfigureConventions</c>. Wire shape stays
///         <c>Guid</c> via <see cref="RoleIdJsonConverter" /> + the <see cref="TypeConverter" />,
///         so route binding and JSON bodies don't see a behavioural change.
///     </para>
///     <para>
///         <see cref="None" /> matches the database <c>NULL</c> semantic the codebase uses today
///         (user without a role).
///     </para>
/// </summary>
[JsonConverter(typeof(RoleIdJsonConverter))]
[TypeConverter(typeof(RoleIdTypeConverter))]
public readonly record struct RoleId(Guid Value)
{
    /// <summary>Sentinel value matching the null-role semantics elsewhere in the codebase.</summary>
    public static RoleId None => new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }
}

/// <summary>
///     System.Text.Json converter so request bodies and response payloads see a plain Guid string,
///     not a <c>{ "value": "..." }</c> object.
/// </summary>
public sealed class RoleIdJsonConverter : JsonConverter<RoleId>
{
    public override RoleId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new RoleId(reader.GetGuid());
    }

    public override void Write(Utf8JsonWriter writer, RoleId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}

/// <summary>
///     ASP.NET Core route-binding converter. Without this, <c>{roleId:guid}</c> route params
///     would fail to bind to <c>RoleId</c>. The <see cref="TypeConverterAttribute" /> on
///     <see cref="RoleId" /> hooks this in automatically.
/// </summary>
public sealed class RoleIdTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || sourceType == typeof(Guid) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        return value switch
        {
            string s => new RoleId(Guid.Parse(s)),
            Guid g => new RoleId(g),
            _ => base.ConvertFrom(context, culture, value)
        };
    }
}