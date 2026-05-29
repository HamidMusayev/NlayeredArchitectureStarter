using API.Middlewares;

namespace API.Extensions;

/// <summary>
///     Wires the <see cref="API.Middlewares.SecurityHeadersMiddleware" /> into the request pipeline.
///     The middleware appends security-hardening HTTP response headers such as
///     <c>X-Content-Type-Options</c>, <c>X-Frame-Options</c>, and <c>Content-Security-Policy</c>.
/// </summary>
public static class SecurityHeadersExtensions
{
    public static WebApplication UseSecurityHeaders(this WebApplication app)
    {
        app.UseMiddleware<SecurityHeadersMiddleware>();
        return app;
    }
}