namespace API.Middlewares;

/// <summary>
///     Adds defensive response headers to every response:
///     <list type="bullet">
///         <item>
///             <c>Strict-Transport-Security</c> — pins HTTPS for a year (browsers ignore over plain HTTP, safe to emit
///             unconditionally).
///         </item>
///         <item><c>X-Content-Type-Options: nosniff</c> — prevents MIME-type sniffing.</item>
///         <item><c>X-Frame-Options: DENY</c> — blocks framing (clickjacking).</item>
///         <item><c>Referrer-Policy: no-referrer</c> — strips the <c>Referer</c> header on outbound nav.</item>
///         <item><c>Permissions-Policy</c> — opts the response out of intrusive browser features.</item>
///         <item><c>Content-Security-Policy</c> — same-origin baseline with carve-outs for the dev UIs.</item>
///     </list>
///     <para>
///         The CSP is intentionally permissive (<c>'unsafe-inline' 'unsafe-eval'</c>) so Swagger UI
///         and GraphQL Voyager render. Tighten in derived projects that don't ship those dev UIs.
///         The deprecated <c>X-XSS-Protection</c> header was dropped — modern browsers ignore it
///         and IE's implementation introduced its own XSS vectors.
///     </para>
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        headers.Append("X-Content-Type-Options", "nosniff");
        headers.Append("X-Frame-Options", "DENY");
        headers.Append("Referrer-Policy", "no-referrer");
        headers.Append("Permissions-Policy",
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
        headers.Append("Content-Security-Policy",
            "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;");

        await next.Invoke(context);
    }
}