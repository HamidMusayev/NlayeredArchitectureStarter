using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CORE.Abstract;
using CORE.Config;
using DTO.User;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace CORE.Concrete;

/// <summary>
///     Default <see cref="IJwtService" /> implementation. Mints HMAC-SHA512 access tokens with
///     the user id (AES-encrypted via <see cref="IEncryptionService" />), name, and role
///     claims; reads + validates the inbound bearer header for the request-scoped getters.
///     Refresh tokens are 64 random bytes base64-encoded — never JWTs themselves.
/// </summary>
public class JwtService(
    ConfigSettings config,
    IHttpContextAccessor context,
    ConfigSettings configSettings,
    IEncryptionService encryptionService)
    : IJwtService
{
    public string CreateTokenForUser(UserToListDto userDto, DateTime expirationDate)
    {
        var claims = new List<Claim>
        {
            new(configSettings.AuthSettings.TokenUserIdKey, encryptionService.Encrypt(userDto.Id.ToString())),
            new(ClaimTypes.Name, userDto.Username),
            new(configSettings.AuthSettings.Role, userDto.Role?.Name ?? string.Empty),
            new(ClaimTypes.Expiration, expirationDate.ToString(CultureInfo.InvariantCulture))
        };

        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configSettings.AuthSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expirationDate,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string? GetTokenString()
    {
        return context.HttpContext?.Request.Headers[config.AuthSettings.HeaderName].ToString();
    }

    public Guid? GetUserIdFromToken()
    {
        var token = GetJwtSecurityToken();
        if (token == null) return null;

        var claim = token.Claims.FirstOrDefault(c => c.Type == config.AuthSettings.TokenUserIdKey);
        if (claim is null || string.IsNullOrEmpty(claim.Value)) return null;

        string decrypted;
        try
        {
            decrypted = encryptionService.Decrypt(claim.Value);
        }
        catch
        {
            // tampered or malformed user-id claim — treat as unauthenticated
            return null;
        }

        return Guid.TryParse(decrypted, out var userId) ? userId : null;
    }

    public bool IsValidToken()
    {
        var tokenString = GetTokenString();

        if (string.IsNullOrEmpty(tokenString) || tokenString.Length < 7) return false;

        var tokenHandler = new JwtSecurityTokenHandler();
        var secretKey = Encoding.ASCII.GetBytes(config.AuthSettings.SecretKey);
        try
        {
            tokenHandler.ValidateToken(tokenString[7..], new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }

    public string TrimToken(string? jwtToken)
    {
        if (string.IsNullOrEmpty(jwtToken) || jwtToken.Length < 7)
            throw new InvalidOperationException("JWT token is missing or malformed.");

        return jwtToken[7..];
    }

    public string? GetRoleFromToken()
    {
        var token = GetJwtSecurityToken();
        if (token == null) return null;

        var roleIdClaim = token.Claims.FirstOrDefault(c => c.Type == config.AuthSettings.Role);

        if (roleIdClaim is null || string.IsNullOrEmpty(roleIdClaim.Value)) return null;

        return roleIdClaim.Value;
    }

    private JwtSecurityToken? GetJwtSecurityToken()
    {
        var tokenString = GetTokenString();

        if (string.IsNullOrEmpty(tokenString)) return null;
        return !tokenString.Contains($"{config.AuthSettings.TokenPrefix} ")
            ? null
            : new JwtSecurityToken(tokenString[7..]);
    }
}