namespace CORE.Config;

/// <summary>
///     Elasticsearch client config. Toggle <see cref="Controllable.IsEnabled" /> in
///     appsettings to opt in — when off the client isn't registered and the optional dependency
///     adds no startup cost.
/// </summary>
public record ElasticSearchSettings : Controllable
{
    public required string Connection { get; set; }
    public required string DefaultIndex { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}