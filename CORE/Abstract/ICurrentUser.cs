using ENTITIES.Identifiers;

namespace CORE.Abstract;

/// <summary>
///     Current actor abstraction — used by <c>AuditableInterceptor</c> and any business logic
///     that needs the "who" without taking a hard dependency on HTTP. The default
///     <c>HttpCurrentUser</c> reads from the JWT; <c>SystemCurrentUser</c> stands in for
///     Hangfire / hosted service scopes. The id is strongly typed (<see cref="UserId" />) so
///     mixing it up with a <c>RoleId</c> / <c>OrganizationId</c> is a compile-time error.
/// </summary>
public interface ICurrentUser
{
    UserId? UserId { get; }
    string? Role { get; }
}