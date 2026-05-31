using DAL.EntityFramework.Abstract;
using Hangfire;

namespace API.HangfireJobs;

/// <summary>
///     Recurring retention job for processed <see cref="ENTITIES.Entities.OutboxMessage" /> rows.
///     Deletes rows where <c>ProcessedOnUtc &lt; now - retentionDays</c> via a single bulk
///     DELETE. Dead-lettered rows are deliberately spared — those need triage, not cleanup.
///     Registered in <c>HangfireExtensions</c>; runs daily by default.
/// </summary>
public class OutboxCleanupJob(IOutboxRepository outboxRepository, ILogger<OutboxCleanupJob> logger)
{
    [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task Run(int retentionDays, IJobCancellationToken hangfireToken)
    {
        if (retentionDays <= 0)
        {
            logger.LogInformation("OutboxCleanupJob skipped: retentionDays={Retention} (≤0 disables pruning)",
                retentionDays);
            return;
        }

        hangfireToken.ThrowIfCancellationRequested();
        var cutoff = DateTimeOffset.UtcNow.AddDays(-retentionDays);

        var deleted = await outboxRepository.PruneProcessedOlderThanAsync(cutoff);
        logger.LogInformation("OutboxCleanupJob: deleted {Deleted} processed rows older than {Cutoff:o}",
            deleted, cutoff);
    }
}