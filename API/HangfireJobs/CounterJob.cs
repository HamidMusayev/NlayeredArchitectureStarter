using Hangfire;

namespace API.HangfireJobs;

/// <summary>
///     Demo recurring Hangfire job. Logs an info entry on every run. Registered as a recurring
///     job in <c>HangfireExtensions</c>. Replace with real background work or remove if not
///     needed in derived projects.
/// </summary>
public class CounterJob(ILogger<CounterJob> logger)
{
    [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public Task Run(IJobCancellationToken hangfireToken)
    {
        try
        {
            hangfireToken.ThrowIfCancellationRequested();
            logger.LogInformation("I am running");
            return Task.CompletedTask;
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("job cancelled");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "job crashed");
            throw;
        }
    }
}