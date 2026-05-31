using ENTITIES.Identifiers;

namespace BLL.Abstract;

/// <summary>
///     Best-effort audit-log writer. Captures the actor (<c>ICurrentUser</c>) and request IP
///     automatically; the caller supplies the dotted action name + optional target. Writes go
///     through a fresh service scope so they don't ride on the caller's <c>IUnitOfWork</c> — an
///     audit row commits even if the surrounding business transaction rolls back, which is
///     what you want for "we tried to do X" forensics.
///     <para>
///         The implementation swallows all exceptions and logs a warning — auditing failure
///         must never break the user-visible flow. Callers can safely <c>await</c> without
///         try/catch.
///     </para>
/// </summary>
public interface IAuditLog
{
    /// <summary>
    ///     Records an action. Pass <paramref name="actorId" /> explicitly when the audit is for
    ///     an event the current user didn't do (e.g. logging a failed login for an email that
    ///     resolved to a user). Leave it null to use <c>ICurrentUser.UserId</c>.
    /// </summary>
    Task LogAsync(
        string action,
        UserId? actorId = null,
        string? targetType = null,
        Guid? targetId = null,
        string? metadata = null,
        CancellationToken ct = default);
}