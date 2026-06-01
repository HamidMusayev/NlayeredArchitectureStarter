using System.Security.Cryptography;
using CORE.Abstract;
using CORE.Config;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CORE.Concrete;

public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 64; // 512 bits
    private const int DefaultIterations = 100_000;

    private readonly int _iterations;

    /// <summary>
    ///     DI constructor — iteration count is read from <see cref="AuthSettings.PasswordIterations" />.
    ///     Marked <see cref="ActivatorUtilitiesConstructorAttribute" /> so the container picks
    ///     this over the <see cref="Pbkdf2PasswordHasher(int)" /> seeder overload without
    ///     ambiguity.
    /// </summary>
    [ActivatorUtilitiesConstructor]
    public Pbkdf2PasswordHasher(IOptions<AuthSettings> options)
        : this(options.Value.PasswordIterations > 0
            ? options.Value.PasswordIterations
            : DefaultIterations)
    {
    }

    /// <summary>
    ///     Direct-iteration overload — useful for seeders / tests / CLI tools that don't have
    ///     bound options. Defaults to the OWASP 2023 floor.
    /// </summary>
    public Pbkdf2PasswordHasher(int iterations = DefaultIterations)
    {
        _iterations = iterations > 0 ? iterations : DefaultIterations;
    }

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
            _iterations,
            HashSizeBytes);

        return Convert.ToBase64String(hashed);
    }
}