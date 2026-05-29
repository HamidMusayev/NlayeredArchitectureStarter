using Redis.OM.Modeling;

namespace ENTITIES.Entities.Redis;

/// <summary>
///     Redis.OM example document. Demonstrates how to attach the <c>Document</c> /
///     <c>Indexed</c> / <c>Searchable</c> attributes so the index-creator hosted service can
///     register schemas at startup. Used by <c>PersonController</c> to show CRUD against Redis
///     without touching Postgres.
/// </summary>
[Document(StorageType = StorageType.Json, Prefixes = new[] { "Person" })]
public class Person
{
    [RedisIdField] [Indexed] public string? Id { get; set; }

    [Indexed] public string? FirstName { get; set; }

    [Indexed] public string? LastName { get; set; }

    [Indexed] public int Age { get; set; }

    [Searchable] public string? PersonalStatement { get; set; }

    [Indexed] public string[] Skills { get; set; } = Array.Empty<string>();

    [Indexed(CascadeDepth = 1)] public Address? Address { get; set; }
}