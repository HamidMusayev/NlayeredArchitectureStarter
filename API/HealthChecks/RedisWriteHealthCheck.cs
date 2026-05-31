using CORE.Config;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace API.HealthChecks;

/// <summary>
///     Verifies Redis accepts writes — <c>SET healthcheck:probe value NX EX 5</c> followed by
///     <c>GET</c> and <c>DEL</c>. The base <c>AddRedis</c> check only PINGs, which passes
///     against a replica that's been demoted to read-only. The 5 s key TTL is belt-and-braces
///     for the case where <c>DEL</c> itself fails — Redis cleans up shortly anyway.
/// </summary>
public sealed class RedisWriteHealthCheck(ConfigSettings config) : IHealthCheck
{
    private const string ProbeKey = "healthcheck:probe";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var connectionString = config.RedisSettings.Connection?.Replace(
            "redis://", string.Empty, StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(connectionString))
            return HealthCheckResult.Healthy("Redis not configured — skipping write probe.");

        try
        {
            // Open a short-lived multiplexer rather than depending on a singleton — the singleton
            // is only registered when CacheSettings.Provider = Redis, but the write probe should
            // run whenever a Redis connection string is configured.
            await using var muxer = await ConnectionMultiplexer.ConnectAsync(connectionString);
            var db = muxer.GetDatabase();

            var written = await db.StringSetAsync(ProbeKey, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                TimeSpan.FromSeconds(5), When.Always);
            if (!written) return HealthCheckResult.Unhealthy("Redis SET returned false.");

            var read = await db.StringGetAsync(ProbeKey);
            if (read.IsNullOrEmpty) return HealthCheckResult.Unhealthy("Redis GET returned empty.");

            await db.KeyDeleteAsync(ProbeKey);

            return HealthCheckResult.Healthy("Redis accepts writes.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis write probe failed.", ex);
        }
    }
}