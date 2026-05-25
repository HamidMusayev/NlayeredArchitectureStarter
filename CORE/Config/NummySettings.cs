namespace CORE.Config;

public record NummySettings
{
    public string ServiceUrl { get; set; } = default!;
    public string ApplicationId { get; set; } = default!;
}
