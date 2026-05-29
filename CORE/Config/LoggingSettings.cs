namespace CORE.Config;

/// <summary>
///     Logging provider selection + Serilog sink configuration.
///     Provider controls which sink(s) receive structured logs:
///     - "Serilog"  → Serilog only (console/file/Seq depending on flags below)
///     - "Nummy"    → Nummy CodeLogger only (legacy)
///     - "Both"     → Serilog and Nummy in parallel (default)
/// </summary>
public record LoggingSettings
{
    public string Provider { get; set; } = "Both";
    public string MinimumLevel { get; set; } = "Information";
    public bool WriteToConsole { get; set; } = true;
    public bool WriteToFile { get; set; }
    public string FilePath { get; set; } = "logs/app-.log";
    public string? SeqUrl { get; set; }
}