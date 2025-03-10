namespace CORE.Config;

public record ElasticSearchSettings : Controllable
{
    public required string Connection { get; set; }
    public required string DefaultIndex { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}