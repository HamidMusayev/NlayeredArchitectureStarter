namespace CORE.Abstract;

/// <summary>
///     Current actor abstraction — used by <c>DataContext.SetAuditProperties</c> and any
///     business logic that needs the "who" without taking a hard dependency on HTTP. The
///     default <c>HttpCurrentUser</c> reads from the JWT; <c>SystemCurrentUser</c> stands in
///     for Hangfire / hosted service scopes.
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Role { get; }
}