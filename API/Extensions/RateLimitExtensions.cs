using System.Threading.RateLimiting;

namespace API.Extensions;

/// <summary>
///     Two rate-limit layers:
///     <list type="bullet">
///         <item>
///             <description>
///                 <b>Global</b> — 5 permits / 10 s per authenticated user (or Host header when
///                 anonymous). Applies to every request and is mostly aimed at smoothing burst
///                 traffic from a single logged-in client.
///             </description>
///         </item>
///         <item>
///             <description>
///                 <b><c>"auth"</c> named policy</b> — 10 permits / 5 min per remote IP, applied
///                 to pre-auth endpoints (<c>login</c>, <c>refresh</c>, <c>otp</c>,
///                 <c>password/reset</c>) via <c>[EnableRateLimiting("auth")]</c>. Slows
///                 credential stuffing / OTP fishing at the source. IP comes from
///                 <see cref="Microsoft.AspNetCore.Http.ConnectionInfo.RemoteIpAddress" /> — if
///                 the API runs behind a proxy, wire <c>UseForwardedHeaders</c> first so this
///                 partitions on the real client IP rather than the proxy's.
///             </description>
///         </item>
///     </list>
/// </summary>
public static class RateLimitExtensions
{
    private const string AuthPolicy = "auth";

    public static IServiceCollection AddRateLimit(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 5,
                        QueueLimit = 2,
                        Window = TimeSpan.FromSeconds(10)
                    }));

            options.AddPolicy(AuthPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(5)
                    }));

            options.OnRejected = (_, _) => new ValueTask();
        });

        return services;
    }

    public static WebApplication UseRateLimit(this WebApplication app)
    {
        app.UseRateLimiter();
        return app;
    }
}