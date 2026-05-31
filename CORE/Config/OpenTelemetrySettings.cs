namespace CORE.Config;

/// <summary>
///     OpenTelemetry tracing + metrics configuration. The OTLP exporter ships spans and metrics
///     to <see cref="OtlpEndpoint" /> (e.g. an OTel collector, Jaeger, or Tempo OTLP receiver).
/// </summary>
public record OpenTelemetrySettings
{
    public string ServiceName { get; set; } = "NlayeredArchitectureStarter.API";
    public string ServiceVersion { get; set; } = "1.0.0";
    public string OtlpEndpoint { get; set; } = "http://localhost:4317";
    public bool EnableTracing { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
}