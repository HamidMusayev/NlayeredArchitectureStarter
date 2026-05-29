namespace CORE.Config;

/// <summary>
///     Logging Serilog sink configuration.
/// </summary>
public record LoggingSettings
{
    public string MinimumLevel { get; set; } = "Information";
    public bool WriteToConsole { get; set; } = true;
    public bool WriteToFile { get; set; }
    public string FilePath { get; set; } = "logs/app-.log";
    public string? SeqUrl { get; set; }
}