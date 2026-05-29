using DTO.User;

namespace DTO.Token;

/// <summary>Outbound representation of a stored token row (used by validation / lookup endpoints).</summary>
public record TokenToListDto(
    Guid Id,
    UserToListDto User,
    string AccessToken,
    DateTimeOffset AccessTokenExpireDate,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpireDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DeletedAt
);