using API.Http;
using CORE.Config;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Refit;
using REFITS.ToDo;

namespace API.Extensions;

/// <summary>
///     Registers Refit HTTP clients with a defensive baseline:
///     <list type="bullet">
///         <item>
///             <description>
///                 <c>AddStandardResilienceHandler</c> — Microsoft.Extensions.Http.Resilience's
///                 five-strategy pipeline (rate-limit → total-timeout → retry → circuit-breaker →
///                 attempt-timeout). Catches transient HTTP failures and stops cascading bad
///                 upstreams from saturating our thread pool.
///             </description>
///         </item>
///         <item>
///             <description>
///                 <see cref="CorrelationIdDelegatingHandler" /> — propagates <c>X-Correlation-Id</c>
///                 so the inbound request id rides on every outbound call.
///             </description>
///         </item>
///     </list>
///     <para>
///         Per-client tuning lives next to its <c>AddRefitClient</c> call. Tighten retry / breaker
///         knobs if the upstream warrants it; the defaults (3 retries, 30 s breaker break-duration,
///         10 s attempt timeout, 30 s total timeout) cover most well-behaved internal APIs.
///     </para>
/// </summary>
public static class RefitExtensions
{
    public static IServiceCollection AddRefitHttpClients(this IServiceCollection services,
        IConfiguration configuration)
    {
        var toDo = configuration.GetConfigSection<ToDoClientSettings>();

        // Transient handler so every Refit client picks up the per-request HttpContextAccessor scope.
        services.AddTransient<CorrelationIdDelegatingHandler>();

        services
            .AddRefitClient<IToDoClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(toDo.BaseUrl))
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
            .AddStandardResilienceHandler(ConfigureStandardResilience);

        return services;
    }

    /// <summary>
    ///     Project-wide defaults for the standard resilience pipeline. Adjust here once rather
    ///     than per-client. Knobs are conservative — favor "don't make the outage worse" over
    ///     "retry harder."
    /// </summary>
    private static void ConfigureStandardResilience(HttpStandardResilienceOptions options)
    {
        // Retry transient failures (network blips, 5xx, 408, 429). The default 3 retries with
        // exponential backoff + jitter is sane; keep it but be explicit so reviewers see it.
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.UseJitter = true;

        // Per-attempt timeout — defends against one slow upstream call holding a thread for a minute.
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);

        // Total-request timeout — outer guard so retries + breaker probes can't compound past this.
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);

        // Open the circuit when ≥50% of recent calls fail, after a 30 s sampling window with at
        // least 10 calls. Stays open 30 s before letting a single probe through.
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.MinimumThroughput = 10;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
    }
}