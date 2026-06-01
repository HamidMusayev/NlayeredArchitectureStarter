using API.Attributes;
using CORE.Abstract;
using CORE.Config;
using Microsoft.Extensions.Options;

namespace API.Middlewares;

/// <summary>
///     Endpoint-aware idempotency wrapper. For requests whose matched endpoint carries
///     <see cref="IdempotentAttribute" />:
///     <list type="bullet">
///         <item>
///             If the configured header is present and the key was seen within TTL — replay the cached response (status,
///             content-type, body) and short-circuit the pipeline.
///         </item>
///         <item>
///             Otherwise buffer the response into memory, let the action run, then cache the bytes keyed by header value
///             before flushing to the wire.
///         </item>
///     </list>
///     Endpoints without the attribute pass through with zero overhead.
///     <para>
///         The key is scoped per <see cref="HttpRequest.Method" /> + <see cref="HttpRequest.Path" />
///         so a key reused across different endpoints can't accidentally collide.
///     </para>
/// </summary>
public sealed class IdempotencyMiddleware(
    RequestDelegate next,
    IIdempotencyStore store,
    IOptions<IdempotencySettings> options)
{
    private readonly IdempotencySettings _settings = options.Value;

    public async Task Invoke(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var hasAttribute = endpoint?.Metadata.GetMetadata<IdempotentAttribute>() is not null;
        if (!hasAttribute)
        {
            await next(context);
            return;
        }

        var headerName = _settings.HeaderName;
        var keyValue = context.Request.Headers[headerName].ToString();
        if (string.IsNullOrWhiteSpace(keyValue))
        {
            await next(context);
            return;
        }

        var scopedKey = $"{context.Request.Method}:{context.Request.Path}:{keyValue}";
        var ct = context.RequestAborted;

        var cached = await store.TryGetAsync(scopedKey, ct);
        if (cached is not null)
        {
            context.Response.StatusCode = cached.StatusCode;
            if (!string.IsNullOrEmpty(cached.ContentType))
                context.Response.ContentType = cached.ContentType;
            context.Response.Headers["Idempotency-Replayed"] = "true";
            await context.Response.Body.WriteAsync(cached.Body, ct);
            return;
        }

        // Buffer downstream response so we can capture+cache it after the action runs.
        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await next(context);

            buffer.Position = 0;
            var bytes = buffer.ToArray();

            // Only cache 2xx outcomes — error replies stay non-idempotent so the client can retry.
            if (context.Response.StatusCode is >= 200 and < 300)
            {
                var ttl = TimeSpan.FromMinutes(Math.Max(1, _settings.TtlMinutes));
                await store.SetAsync(scopedKey,
                    new IdempotentResponse(context.Response.StatusCode, context.Response.ContentType, bytes),
                    ttl, ct);
            }

            // Flush buffered bytes to the real response body.
            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody, ct);
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }
}