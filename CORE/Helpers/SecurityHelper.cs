using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CORE.Abstract;
using CORE.Config;
using DTO.User;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;

namespace CORE.Helpers;

public class SecurityHelper(ConfigSettings configSettings, IUtilService utilService)
{
    public static string GenerateSalt()
    {
        var saltBytes = new byte[16];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }

        return Convert.ToBase64String(saltBytes);
    }

    public static string HashPassword(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);

        var hashed = KeyDerivation.Pbkdf2(
            password,
            saltBytes,
            KeyDerivationPrf.HMACSHA512,
            100000,
            512 / 8);

        return Convert.ToBase64String(hashed);
    }

    public string CreateTokenForUser(UserToListDto userDto, DateTime expirationDate)
    {
        var claims = new List<Claim>
        {
            new(configSettings.AuthSettings.TokenUserIdKey, utilService.Encrypt(userDto.Id.ToString())),
            new(ClaimTypes.Name, userDto.Username),
            new(configSettings.AuthSettings.Role, userDto.Role == null ? string.Empty : userDto.Role!.Name),
            new(ClaimTypes.Expiration, expirationDate.ToString(CultureInfo.InvariantCulture))
        };
        //claims.Add(new Claim(_configSettings.AuthSettings.TokenUserTypeKey, userDto.Type.ToString()));

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
}