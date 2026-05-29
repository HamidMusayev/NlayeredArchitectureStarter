namespace API.Middlewares;

/// <summary>
///     Adds defensive response headers (X-Content-Type-Options, X-Frame-Options, X-XSS-Protection,
///     Referrer-Policy, Content-Security-Policy) to every response.
///     <para>
///         The CSP is intentionally permissive (<c>'unsafe-inline' 'unsafe-eval'</c>) to let Swagger
///         UI and GraphQL Voyager render — tighten in derived projects that don't ship those dev UIs.
///     </para>
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers.Append("X-Content-Type-Options", "nosniff");
        headers.Append("X-Frame-Options", "Deny");
        headers.Append("X-XSS-Protection", "1; mode=block");
        headers.Append("Referrer-Policy", "no-referrer");
        headers.Append("Content-Security-Policy",
            "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;");

        await next.Invoke(context);
    }
}