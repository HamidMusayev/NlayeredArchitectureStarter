namespace CORE.Abstract;

/// <summary>
///     Password hashing surface — salt generation + deterministic hash. Default
///     <c>Pbkdf2PasswordHasher</c> uses PBKDF2-HMAC-SHA512 with the iteration count from
///     <see cref="CORE.Config.AuthSettings.PasswordIterations" />.
/// </summary>
public interface IPasswordHasher
{
    string GenerateSalt();
    string Hash(string password, string salt);
}