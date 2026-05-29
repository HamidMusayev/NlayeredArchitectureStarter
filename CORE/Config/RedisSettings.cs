namespace CORE.Config;

/// <summary>Redis connection string (Redis.OM-style <c>redis://host:port</c>).</summary>
public record RedisSettings
{
    public required string Connection { get; set; }
}
