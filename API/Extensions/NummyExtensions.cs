using System.Net;
using CORE.Config;
using CORE.Localization;
using DTO.Responses;
using Nummy.CodeLogger.Extensions;
using Nummy.ExceptionHandler.Extensions;
using Nummy.HealthChecker.Entites;
using Nummy.HealthChecker.Extensions;
using Nummy.HttpLogger.Extensions;

namespace API.Extensions;

/// <summary>
///     Nummy CodeLogger + HttpLogger + ExceptionHandler + HealthChecker. Kept in parallel with
///     Serilog + OpenTelemetry (see <see cref="ObservabilityExtensions" />) and ProblemDetails
///     (see <see cref="ProblemDetailsExtensions" />) — none of those replace Nummy.
/// </summary>
public static class NummyExtensions
{
    public static IServiceCollection AddNummyObservability(this IServiceCollection services, ConfigSettings config)
    {
        services.AddNummyCodeLogger(options =>
        {
            options.NummyServiceUrl = config.NummySettings.ServiceUrl;
            options.ApplicationId = config.NummySettings.ApplicationId;
        });

        services.AddNummyHttpLogger(options =>
        {
            options.EnableRequestLogging = true;
            options.EnableResponseLogging = true;
            options.ExcludeContainingPaths = ["swagger"];
            options.ApplicationId = config.NummySettings.ApplicationId;
            options.NummyServiceUrl = config.NummySettings.ServiceUrl;
        });

        services.AddNummyExceptionHandler(options =>
        {
            options.HandleException = true;
            options.ResponseStatusCode = HttpStatusCode.Conflict;
            options.Response = new ErrorResult(Messages.GeneralError.Translate());
            options.ApplicationId = config.NummySettings.ApplicationId;
            options.NummyServiceUrl = config.NummySettings.ServiceUrl;
        });

        services.AddNummyHealthChecker(options =>
        {
            options.Path = "nummy/health";
            options.CheckAsync = (_, _) => Task.FromResult(new NummyHealthResult
            {
                IsHealthy = true,
                Message = "Service is healthy"
            });
        });

        return services;
    }

    public static WebApplication UseNummyObservability(this WebApplication app)
    {
        app.UseNummyExceptionHandler();
        app.UseNummyHttpLogger();
        app.MapNummyHealthChecker();
        return app;
    }
}