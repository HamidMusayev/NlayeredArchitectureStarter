namespace CORE.Config;

public record ConnectionStrings
{
    public required string AppDb { get; set; }
    public required string AppNummyDb { get; set; }
}