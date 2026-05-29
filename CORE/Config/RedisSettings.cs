namespace CORE.Config;

/// <summary>
///     Redis connection string (Redis.OM-style <c>redis://host:port</c>). Toggled by
///     <see cref="Controllable.IsEnabled" />.
/// </summary>
public record RedisSettings : Controllable
{
    public required string Connection { get; set; }
}