using System.Diagnostics;
using API.Middlewares;

namespace API.Extensions;

/// <summary>
///     Registers RFC 7807 ProblemDetails support. Augments every problem response with a
///     <c>traceId</c> and <c>correlationId</c> extension. <c>UseProblemDetailsExceptions</c>
///     activates the exception handler and status-code pages middlewares.
/// </summary>
public static class ProblemDetailsExtensions
{
    public static IServiceCollection AddCoreProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                ctx.ProblemDetails.Extensions["traceId"] = Activity.Current?.Id ?? ctx.HttpContext.TraceIdentifier;

                if (ctx.HttpContext.Items["CorrelationId"] is string correlationId)
                    ctx.ProblemDetails.Extensions["correlationId"] = correlationId;
            };
        });

        services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

        return services;
    }

    public static WebApplication UseProblemDetailsExceptions(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        return app;
    }
}