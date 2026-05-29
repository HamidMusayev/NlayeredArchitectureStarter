namespace CORE.Config;

/// <summary>Nummy CodeLogger / HttpLogger / ExceptionHandler / HealthChecker shared settings.</summary>
public record NummySettings
{
    public string ServiceUrl { get; set; } = default!;
    public string ApplicationId { get; set; } = default!;
}