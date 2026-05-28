using System.Security.Cryptography;
using System.Text;
using CORE.Abstract;
using CORE.Config;

namespace CORE.Concrete;

public class AesEncryptionService(ConfigSettings config) : IEncryptionService
{
    public string Encrypt(string value)
    {
        var keyBytes = Encoding.UTF8.GetBytes(config.CryptographySettings.KeyBase64);
        var ivBytes = Encoding.UTF8.GetBytes(config.CryptographySettings.VBase64);

        using var aes = Aes.Create();
        using var transform = aes.CreateEncryptor(keyBytes, ivBytes);

        var inputBuffer = Encoding.Unicode.GetBytes(value);
        var outputBuffer = transform.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
        return Convert.ToBase64String(outputBuffer);
    }

    public string Decrypt(string value)
    {
        var keyBytes = Encoding.UTF8.GetBytes(config.CryptographySettings.KeyBase64);
        var ivBytes = Encoding.UTF8.GetBytes(config.CryptographySettings.VBase64);

        using var aes = Aes.Create();
        using var transform = aes.CreateDecryptor(keyBytes, ivBytes);

        var inputBuffer = Convert.FromBase64String(value);
        var outputBuffer = transform.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
        return Encoding.Unicode.GetString(outputBuffer);
    }
}