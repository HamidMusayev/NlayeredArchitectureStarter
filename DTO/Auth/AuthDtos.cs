using DTO.User;

namespace DTO.Auth;

public record LoginDto(string Email, string Password);

public record LoginResponseDto(
    UserToListDto User,
    string AccessToken,
    DateTime AccessTokenExpireDate,
    string RefreshToken,
    DateTime RefreshTokenExpireDate
);

public record ResetPasswordDto(
    string Email,
    string? VerificationCode,
    string Password,
    string PasswordConfirmation
);