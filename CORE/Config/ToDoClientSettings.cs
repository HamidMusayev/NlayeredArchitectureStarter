namespace CORE.Config;

/// <summary>Refit ToDo client config — sample showing how to wire an external HTTP API into the starter.</summary>
public class ToDoClientSettings
{
    public required string BaseUrl { get; set; }
    public required string ClientSecret { get; set; }
    public required string ClientKey { get; set; }
}