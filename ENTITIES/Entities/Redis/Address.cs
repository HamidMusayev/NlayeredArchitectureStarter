using Redis.OM.Modeling;

namespace ENTITIES.Entities.Redis;

/// <summary>
///     Embedded Redis.OM sub-document for <see cref="Person" />. Shows how nested objects work
///     with Redis.OM's <c>CascadeDepth</c> indexing — the parent's <c>[Indexed(CascadeDepth = 1)]</c>
///     makes these fields filterable too.
/// </summary>
public class Address
{
    [Indexed] public int? StreetNumber { get; set; }

    [Indexed] public string? Unit { get; set; }

    [Searchable] public string? StreetName { get; set; }

    [Indexed] public string? City { get; set; }

    [Indexed] public string? State { get; set; }

    [Indexed] public string? PostalCode { get; set; }

    [Indexed] public string? Country { get; set; }

    [Indexed] public GeoLoc Location { get; set; }
}