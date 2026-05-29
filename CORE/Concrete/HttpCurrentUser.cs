using CORE.Abstract;

namespace CORE.Concrete;

/// <summary>
///     HTTP-bound <see cref="ICurrentUser" /> — pulls user id + role from the JWT on the
///     inbound request via <see cref="IJwtService" />. Use <see cref="SystemCurrentUser" />
///     instead in non-HTTP scopes (Hangfire jobs, hosted services).
/// </summary>
public class HttpCurrentUser(IJwtService jwtService) : ICurrentUser
{
    public Guid? UserId => jwtService.GetUserIdFromToken();
    public string? Role => jwtService.GetRoleFromToken();
}