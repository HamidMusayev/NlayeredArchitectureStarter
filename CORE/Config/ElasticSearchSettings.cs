namespace CORE.Config;

/// <summary>Elasticsearch client config.</summary>
public record ElasticSearchSettings
{
    public required string Connection { get; set; }
    public required string DefaultIndex { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}
