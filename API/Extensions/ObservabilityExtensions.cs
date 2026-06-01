using API.Middlewares;
using CORE.Config;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace API.Extensions;

/// <summary>
///     Serilog (structured logging) + OpenTelemetry (tracing/metrics) + correlation ID.
/// </summary>
public static class ObservabilityExtensions
{
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        var settings = builder.Configuration.GetConfigSection<LoggingSettings>();
        var otel = builder.Configuration.GetConfigSection<OpenTelemetrySettings>();

        var level = Enum.TryParse<LogEventLevel>(settings.MinimumLevel, true, out var parsed)
            ? parsed
            : LogEventLevel.Information;

        builder.Host.UseSerilog((ctx, _, cfg) =>
        {
            cfg
                .MinimumLevel.Is(level)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Application", otel.ServiceName)
                .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName);

            if (settings.WriteToConsole)
                cfg.WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {SourceContext} {Message:lj}{NewLine}{Exception}");

            if (settings.WriteToFile)
                cfg.WriteTo.File(
                    settings.FilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14,
                    outputTemplate:
                    "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {CorrelationId} {SourceContext} {Message:lj}{NewLine}{Exception}");

            if (!string.IsNullOrWhiteSpace(settings.SeqUrl)) cfg.WriteTo.Seq(settings.SeqUrl);
        });

        return builder;
    }

    public static IServiceCollection AddOpenTelemetryObservability(this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetConfigSection<OpenTelemetrySettings>();

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(settings.ServiceName, serviceVersion: settings.ServiceVersion);

        var otel = services.AddOpenTelemetry();
        otel.ConfigureResource(r => r.AddService(settings.ServiceName, serviceVersion: settings.ServiceVersion));

        if (settings.EnableTracing)
            otel.WithTracing(t => t
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddOtlpExporter(o => o.Endpoint = new Uri(settings.OtlpEndpoint)));

        if (settings.EnableMetrics)
            otel.WithMetrics(m => m
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(o => o.Endpoint = new Uri(settings.OtlpEndpoint)));

        return services;
    }

    public static WebApplication UseObservability(this WebApplication app)
    {
        // Correlation ID must run before request logging so every log line carries the ID.
        app.UseMiddleware<CorrelationIdMiddleware>();

        app.UseSerilogRequestLogging();

        return app;
    }
}