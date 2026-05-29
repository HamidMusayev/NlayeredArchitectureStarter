using System.Threading.RateLimiting;

namespace API.Containers.Extensions;

/// <summary>
///     Registers a global fixed-window rate limiter: 5 permits per 10-second window per
///     authenticated user name (or host header for anonymous requests), with a queue of 2 and
///     HTTP 429 on rejection. <c>UseRateLimit</c> activates the limiter middleware.
/// </summary>
public static class RateLimitExtensions
{
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