namespace CORE.Config;

/// <summary>MongoDB driver config — toggle <see cref="Controllable.IsEnabled" /> to opt in.</summary>
public record MongoDbSettings : Controllable
{
    public required string Connection { get; set; }
    public required string Database { get; set; }
}