using DAL.EntityFramework.Abstract;
using Hangfire;

namespace API.HangfireJobs;

/// <summary>
///     Recurring retention job for <see cref="ENTITIES.Entities.AuditLog" />. Deletes rows
///     older than <c>AuditLogSettings.RetentionDays</c> via a single bulk DELETE. Registered
///     in <c>HangfireExtensions</c>; runs daily by default. Loud-but-not-fatal: a failure
///     just defers the prune by one cycle.
/// </summary>
public class AuditLogPruneJob(IAuditLogRepository auditLogRepository, ILogger<AuditLogPruneJob> logger)
{
    [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task Run(int retentionDays, IJobCancellationToken hangfireToken)
    {
        if (retentionDays <= 0)
        {
            logger.LogInformation("AuditLogPruneJob skipped: retentionDays={Retention} (≤0 disables pruning)",
                retentionDays);
            return;
        }

        hangfireToken.ThrowIfCancellationRequested();
        var cutoff = DateTimeOffset.UtcNow.AddDays(-retentionDays);

        var deleted = await auditLogRepository.PruneOlderThanAsync(cutoff);
        logger.LogInformation("AuditLogPruneJob: deleted {Deleted} rows older than {Cutoff:o}", deleted, cutoff);
    }
}