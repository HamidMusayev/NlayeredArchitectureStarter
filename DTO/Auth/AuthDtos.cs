using DTO.User;

namespace DTO.Auth;

/// <summary>Inbound login credentials.</summary>
public record LoginDto(string Email, string Password);

/// <summary>Outbound login response carrying both halves of the JWT pair + the authenticated user.</summary>
public record LoginResponseDto(
    UserToListDto User,
    string AccessToken,
    DateTime AccessTokenExpireDate,
    string RefreshToken,
    DateTime RefreshTokenExpireDate
);

/// <summary>
///     Inbound reset-password payload. <see cref="VerificationCode" /> is the OTP previously
///     emailed by <c>IAccountRecoveryService.SendOtpAsync</c>; the service compares it against
///     the stored hash and rotates the password on match.
/// </summary>
public record ResetPasswordDto(
    string Email,
    string? VerificationCode,
    string Password,
    string PasswordConfirmation
);