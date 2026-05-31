namespace CORE.Config;

/// <summary>MongoDB driver config.</summary>
public record MongoDbSettings
{
    public required string Connection { get; set; }
    public required string Database { get; set; }
}