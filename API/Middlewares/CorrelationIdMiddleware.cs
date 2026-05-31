using System.Diagnostics;
using CORE.Concrete.Observability;
using Serilog.Context;

namespace API.Middlewares;

/// <summary>
///     Reads <c>X-Correlation-Id</c> off the inbound request (or generates one), then:
///     <list type="bullet">
///         <item>Sets it on <see cref="Activity.Current" /> so OpenTelemetry spans carry it as a baggage tag.</item>
///         <item>
///             Pushes it onto Serilog's <see cref="LogContext" /> so every log line inside the request includes
///             <c>CorrelationId</c>.
///         </item>
///         <item>Echoes it back on the response header so clients can correlate too.</item>
///     </list>
///     Used together with <c>CorrelationIdDelegatingHandler</c> on Refit clients to propagate the
///     same ID to downstream HTTP calls without per-call code changes.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";
    private const string LogContextProperty = CorrelationContext.Key;

    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing) &&
                            !string.IsNullOrWhiteSpace(existing.ToString())
            ? existing.ToString()
            : Guid.NewGuid().ToString("N");

        context.Items[LogContextProperty] = correlationId;
        context.Response.OnStarting(() =>
        {
            // Re-set in case earlier middleware already populated headers.
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        Activity.Current?.SetBaggage(LogContextProperty, correlationId);
        Activity.Current?.SetTag("correlation.id", correlationId);

        using (LogContext.PushProperty(LogContextProperty, correlationId))
        {
            await next.Invoke(context);
        }
    }
}