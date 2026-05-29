namespace CORE.Config;

/// <summary>
///     AES symmetric encryption parameters used by <c>AesEncryptionService</c> — base64-encoded
///     key + IV. Treat both fields as secrets: move out of <c>appsettings.json</c> into User
///     Secrets / KMS in real deployments.
/// </summary>
public class CryptographySettings
{
    public required string KeyBase64 { get; set; }
    public required string VBase64 { get; set; }
}