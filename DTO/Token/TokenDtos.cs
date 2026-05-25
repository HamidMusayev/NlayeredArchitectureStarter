using DTO.User;

namespace DTO.Token;

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