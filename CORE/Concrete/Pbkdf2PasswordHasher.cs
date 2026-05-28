using System.Security.Cryptography;
using CORE.Abstract;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace CORE.Concrete;

public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSizeBytes = 16;
    private const int Iterations = 100_000;
    private const int HashSizeBytes = 64; // 512 bits

    public string GenerateSalt()
    {
        var saltBytes = new byte[SaltSizeBytes];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    public string Hash(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);

        var hashed = KeyDerivation.Pbkdf2(
            password,
            saltBytes,
            KeyDerivationPrf.HMACSHA512,
            Iterations,
            HashSizeBytes);

        return Convert.ToBase64String(hashed);
    }
}