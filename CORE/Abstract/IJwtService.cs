using DTO.User;

namespace CORE.Abstract;

public interface IJwtService
{
    string CreateTokenForUser(UserToListDto user, DateTime expirationDate);
    public string? GetTokenString();
    public bool IsValidToken();
    public Guid? GetUserIdFromToken();
    public string GenerateRefreshToken();
    public string? GetRoleFromToken();
    public string TrimToken(string? jwtToken);
}