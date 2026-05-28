using CORE.Abstract;

namespace CORE.Concrete;

public class HttpCurrentUser(IJwtService jwtService) : ICurrentUser
{
    public Guid? UserId => jwtService.GetUserIdFromToken();
    public string? Role => jwtService.GetRoleFromToken();
}