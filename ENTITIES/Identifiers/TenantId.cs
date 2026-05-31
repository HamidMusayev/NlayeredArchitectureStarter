using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ENTITIES.Identifiers;

/// <summary>
///     Strongly-typed identifier for a tenant. Wraps a <see cref="Guid" /> but doesn't implicitly
///     convert to or from one — passing a <c>UserId</c> where a <c>TenantId</c> is expected (or
///     vice-versa) becomes a compile-time error instead of a silent bug.
///     <para>
///         Storage shape stays <c>uuid</c> in Postgres via <c>HasConversion</c> registered in
///         <c>DataContext.OnModelCreating</c>. Wire shape stays <c>Guid</c> via
///         <see cref="TenantIdJsonConverter" /> + the <see cref="TypeConverter" /> wired below,
///         so route binding and JSON bodies don't see a behavioural change.
///     </para>
///     <para>
///         <see cref="None" /> represents "no tenant" and matches the database <c>NULL</c>
///         semantic the codebase uses today (shared / non-tenant rows or unresolved requests).
///     </para>
/// </summary>
[JsonConverter(typeof(TenantIdJsonConverter))]
[TypeConverter(typeof(TenantIdTypeConverter))]
public readonly record struct TenantId(Guid Value)
{
    /// <summary>Sentinel value matching the null-tenant semantics elsewhere in the codebase.</summary>
    public static TenantId None => new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }
}

/// <summary>
///     System.Text.Json converter so request bodies and response payloads see a plain Guid string,
///     not a <c>{ "value": "..." }</c> object. Symmetric: reads + writes the bare Guid form.
/// </summary>
public sealed class TenantIdJsonConverter : JsonConverter<TenantId>
{
    public override TenantId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new TenantId(reader.GetGuid());
    }

    public override void Write(Utf8JsonWriter writer, TenantId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}

/// <summary>
///     ASP.NET Core route-binding converter. Without this, <c>{tenantId:guid}</c> route params
///     would fail to bind to <c>TenantId</c>. The <see cref="TypeConverterAttribute" /> on
///     <see cref="TenantId" /> hooks this in automatically.
/// </summary>
public sealed class TenantIdTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || sourceType == typeof(Guid) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        return value switch
        {
            string s => new TenantId(Guid.Parse(s)),
            Guid g => new TenantId(g),
            _ => base.ConvertFrom(context, culture, value)
        };
    }
}