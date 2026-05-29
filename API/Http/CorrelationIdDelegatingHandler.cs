using API.Middlewares;

namespace API.Http;

/// <summary>
///     <see cref="DelegatingHandler" /> that copies the current request's correlation ID
///     onto outbound HTTP calls. Wired into Refit clients via
///     <c>.AddHttpMessageHandler&lt;CorrelationIdDelegatingHandler&gt;()</c> so downstream
///     services can trace the same logical request.
/// </summary>
public class CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var ctx = httpContextAccessor.HttpContext;
        if (ctx?.Items["CorrelationId"] is string correlationId &&
            !request.Headers.Contains(CorrelationIdMiddleware.HeaderName))
            request.Headers.Add(CorrelationIdMiddleware.HeaderName, correlationId);

        return base.SendAsync(request, cancellationToken);
    }
}