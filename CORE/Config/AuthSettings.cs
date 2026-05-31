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

    /// <summary>
    ///     Access-token lifetime. Industry default is short — 15 minutes — so a leaked JWT has
    ///     a tight blast radius. The token introspection cache + refresh-token rotation handle
    ///     the resulting per-request renewal traffic at sub-ms cost. Bumping this is fine but
    ///     consider what happens if a logged-in laptop is stolen.
    /// </summary>
    public required int AccessTokenLifetimeMinutes { get; set; }

    /// <summary>
    ///     Refresh-token absolute lifetime (not relative to access expiry). Industry default is
    ///     much longer than the access token — 14 days = 20160 minutes — because rotation is
    ///     used to keep the user signed in across browser restarts. Reuse-detection
    ///     (<c>TokenService.RotateAsync</c>) burns the family on replay, so a stolen refresh
    ///     token gets invalidated quickly anyway.
    /// </summary>
    public required int RefreshTokenLifetimeMinutes { get; set; }

    /// <summary>
    ///     PBKDF2 iteration count used by IPasswordHasher. Default 100_000
    ///     (OWASP 2023 floor for HMAC-SHA512). Bump every couple of years as hardware
    ///     gets faster — re-evaluate at the same cadence you rotate signing keys.
    /// </summary>
    public int PasswordIterations { get; set; } = 100_000;

    /// <summary>
    ///     Consecutive failed logins before the account is locked. 0 disables lockout.
    ///     Default 5 — slows brute-force without locking out the average fat-fingered user.
    /// </summary>
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>
    ///     Minutes an account stays locked after exceeding <see cref="MaxFailedLoginAttempts" />.
    ///     Default 30 — long enough to make scripted attacks expensive, short enough that a
    ///     genuine user can wait it out.
    /// </summary>
    public int LockoutDurationMinutes { get; set; } = 30;
}