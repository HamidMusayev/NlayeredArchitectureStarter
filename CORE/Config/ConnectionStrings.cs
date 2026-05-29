namespace CORE.Config;

/// <summary>Database connection strings — primary app DB </summary>
public record ConnectionStrings
{
    public required string AppDb { get; set; }
}