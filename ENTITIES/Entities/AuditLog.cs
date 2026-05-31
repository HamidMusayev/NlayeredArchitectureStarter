using ENTITIES.Entities.Generic;
using ENTITIES.Identifiers;

namespace ENTITIES.Entities;

/// <summary>
///     Append-only record of a sensitive action — login (success/failure/lockout), logout,
///     password change, role assignment, permission grant, user delete. Deliberately does
///     <b>not</b> extend <c>Auditable</c>: the row IS the audit, soft-deleting / editing it
///     would defeat the purpose. Rows are pruned in bulk by a retention job after
///     <c>AuditLogSettings.RetentionDays</c>.
/// </summary>
public class AuditLog : IEntity
{
    public Guid Id { get; set; }

    /// <summary>Actor's user id. Null for anonymous events (e.g. login failure on unknown email).</summary>
    public UserId? UserId { get; set; }

    /// <summary>
    ///     Dotted lowercase identifier — <c>auth.login.success</c>, <c>auth.login.failed</c>,
    ///     <c>auth.login.locked</c>, <c>auth.logout</c>, <c>auth.password.reset</c>,
    ///     <c>user.role.assigned</c>, <c>permission.granted</c>, <c>user.deleted</c>, etc.
    ///     Picking a stable convention up front makes BI queries trivial later.
    /// </summary>
    public required string Action { get; set; }

    /// <summary>Domain type of the target (e.g. <c>User</c>, <c>Role</c>). Null when the action is actor-scoped.</summary>
    public string? TargetType { get; set; }

    /// <summary>Id of the targeted entity. Null for actor-scoped or anonymous actions.</summary>
    public Guid? TargetId { get; set; }

    /// <summary>Remote IP captured from <c>HttpContext.Connection</c>. Null for non-HTTP callers (jobs, CLI).</summary>
    public string? IpAddress { get; set; }

    /// <summary>
    ///     Free-form JSON for context — keep it small. PII goes through this lens too: only what you'd defend in a
    ///     breach.
    /// </summary>
    public string? Metadata { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}