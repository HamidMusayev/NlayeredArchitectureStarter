using System.Security.Cryptography;
using System.Text;
using CORE.Abstract;
using CORE.Config;
using Microsoft.Extensions.Options;

namespace CORE.Concrete;

/// <summary>
///     AES symmetric encryption with key + IV pulled from <see cref="CryptographySettings" />.
///     Used to obfuscate the user-id claim inside JWTs so they're opaque to clients but still
///     round-trippable server-side.
/// </summary>
public class AesEncryptionService(IOptions<CryptographySettings> options) : IEncryptionService
{
    private readonly CryptographySettings _settings = options.Value;

    public string Encrypt(string value)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_settings.KeyBase64);
        var ivBytes = Encoding.UTF8.GetBytes(_settings.VBase64);

        using var aes = Aes.Create();
        using var transform = aes.CreateEncryptor(keyBytes, ivBytes);

        var inputBuffer = Encoding.Unicode.GetBytes(value);
        var outputBuffer = transform.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
        return Convert.ToBase64String(outputBuffer);
    }

    public string Decrypt(string value)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_settings.KeyBase64);
        var ivBytes = Encoding.UTF8.GetBytes(_settings.VBase64);

        using var aes = Aes.Create();
        using var transform = aes.CreateDecryptor(keyBytes, ivBytes);

        var inputBuffer = Convert.FromBase64String(value);
        var outputBuffer = transform.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
        return Encoding.Unicode.GetString(outputBuffer);
    }
}