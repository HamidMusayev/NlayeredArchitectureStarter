﻿using DTO.User;

namespace DTO.Token;

public record RefreshTokenDto(string AccessToken, string RefreshToken);

public record TokenToAddDto(int UserId, string AccessToken, DateTimeOffset AccessTokenExpireDate,
    string RefreshToken, DateTimeOffset RefreshTokenExpireDate);

public record TokenToListDto(int TokenId, UserToListDto User, string AccessToken, DateTimeOffset AccessTokenExpireDate,
    string RefreshToken, DateTimeOffset RefreshTokenExpireDate, DateTimeOffset CreatedAt, DateTimeOffset? DeletedAt);