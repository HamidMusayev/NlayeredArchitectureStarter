namespace CORE.Config;

/// <summary>
///     Audit-log retention configuration. The <c>AuditLogPruneJob</c> recurring Hangfire job
///     reads <see cref="RetentionDays" /> at run time and bulk-deletes rows older than that.
/// </summary>
public record AuditLogSettings
{
    /// <summary>Days of audit history to retain. Default 365. Set to 0 to disable the prune.</summary>
    public int RetentionDays { get; set; } = 365;
}