namespace CORE.Config;

/// <summary>
///     JWT bearer authentication settings — secret key, header conventions, token lifetimes,
///     and PBKDF2 iteration count for the password hasher. Secrets here (especially
///     <see cref="SecretKey" />) must be provided via User Secrets or platform secret stores
///     in real deployments.
/// </summary>
public record AuthSettings
{
    public required string Type { get; set; }
    public required string HeaderName { get; set; }
    public required string Role { get; set; }
    public required string RefreshTokenHeaderName { get; set; }
    public required string TokenPrefix { get; set; }
    public required string ContentType { get; set; }
    public required string SecretKey { get; set; }
    public required string TokenUserIdKey { get; set; }
    public required int TokenExpirationTimeInHours { get; set; }
    public required int RefreshTokenAdditionalMinutes { get; set; }

    /// <summary>
    ///     PBKDF2 iteration count used by IPasswordHasher. Default 100_000
    ///     (OWASP 2023 floor for HMAC-SHA512). Bump every couple of years as hardware
    ///     gets faster — re-evaluate at the same cadence you rotate signing keys.
    /// </summary>
    public int PasswordIterations { get; set; } = 100_000;
}