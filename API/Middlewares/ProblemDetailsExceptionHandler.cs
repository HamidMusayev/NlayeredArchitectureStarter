using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace API.Middlewares;

/// <summary>
///     Converts unhandled exceptions into RFC 7807 <see cref="ProblemDetails" /> responses.
///     Enriches the payload with <c>traceId</c> (from <see cref="Activity.Current" />) and
///     <c>correlationId</c> (set by <see cref="CorrelationIdMiddleware" />) so log lines
///     and API responses can be cross-referenced.
///     <para>
///         Registered via <c>services.AddExceptionHandler&lt;ProblemDetailsExceptionHandler&gt;()</c>
///         and activated by <c>app.UseExceptionHandler()</c>.
///     </para>
/// </summary>
public class ProblemDetailsExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ProblemDetailsExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception for {Method} {Path}",
            httpContext.Request.Method, httpContext.Request.Path);

        var (status, title) = MapException(exception);

        httpContext.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = exception.Message,
            Type = $"https://httpstatuses.io/{status}",
            Instance = httpContext.Request.Path
        };

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }

    private static (int Status, string Title) MapException(Exception ex)
    {
        return ex switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Invalid operation"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            NotImplementedException => (StatusCodes.Status501NotImplemented, "Not implemented"),
            TimeoutException => (StatusCodes.Status504GatewayTimeout, "Upstream timeout"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }
}