namespace CORE.Config;

/// <summary>Database connection strings — primary app DB + the optional Nummy logging DB.</summary>
public record ConnectionStrings
{
    public required string AppDb { get; set; }
    public required string AppNummyDb { get; set; }
}