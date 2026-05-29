namespace CORE.Abstract;

/// <summary>
///     Symmetric encrypt/decrypt for opaque round-trip values (e.g. obfuscated ids in JWT
///     claims). Default <c>AesEncryptionService</c> uses AES with the key/IV from
///     <see cref="CORE.Config.CryptographySettings" />.
/// </summary>
public interface IEncryptionService
{
    string Encrypt(string value);
    string Decrypt(string value);
}