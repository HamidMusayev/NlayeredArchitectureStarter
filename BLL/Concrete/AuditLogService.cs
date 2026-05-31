using BLL.Abstract;
using CORE.Abstract;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using ENTITIES.Entities;
using ENTITIES.Identifiers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BLL.Concrete;

/// <summary>
///     Default <see cref="IAuditLog" /> implementation. Each <c>LogAsync</c> call opens its own
///     <see cref="IServiceScope" /> so the audit row is committed independently of the caller's
///     <see cref="IUnitOfWork" /> — keeps "we tried" forensics even when the business write
///     rolls back. All exceptions are caught + logged; auditing failure never propagates to
///     the caller.
/// </summary>
public sealed class AuditLogService(
    IServiceScopeFactory scopeFactory,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuditLogService> logger)
    : IAuditLog
{
    public async Task LogAsync(
        string action,
        UserId? actorId = null,
        string? targetType = null,
        Guid? targetId = null,
        string? metadata = null,
        CancellationToken ct = default)
    {
        // Snapshot request-scoped data before opening the new scope — the HttpContext is the
        // caller's, not the new scope's.
        var ip = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var sp = scope.ServiceProvider;

            // Honor an explicit actorId; otherwise fall back to ICurrentUser within the same
            // request scope as the caller (resolved via the parent HttpContext).
            var resolvedActorId = actorId
                                  ?? httpContextAccessor.HttpContext?.RequestServices
                                      .GetService<ICurrentUser>()?.UserId;

            var entry = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = resolvedActorId,
                Action = action,
                TargetType = targetType,
                TargetId = targetId,
                IpAddress = ip,
                Metadata = metadata,
                OccurredAt = DateTimeOffset.UtcNow
            };

            var repo = sp.GetRequiredService<IAuditLogRepository>();
            var uow = sp.GetRequiredService<IUnitOfWork>();
            await repo.AddAsync(entry);
            await uow.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "AuditLog write failed (action={Action}, targetType={TargetType}, targetId={TargetId})",
                action, targetType, targetId);
        }
    }
}