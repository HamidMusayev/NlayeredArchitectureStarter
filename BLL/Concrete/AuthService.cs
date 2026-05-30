using AutoMapper;
using BLL.Abstract;
using CORE.Abstract;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using DTO.Auth;
using DTO.Responses;
using DTO.User;

namespace BLL.Concrete;

/// <summary>
///     Default <see cref="IAuthService" /> implementation. Validates credentials with PBKDF2
///     comparison, resolves the user from a JWT claim for token-based re-login, and soft-deletes
///     all associated token rows on logout.
/// </summary>
public class AuthService(
    IUserRepository userRepository,
    ITokenRepository tokenRepository,
    ITokenIntrospectionCache introspectionCache,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IJwtService jwtService,
    IPasswordHasher passwordHasher)
    : IAuthService
{
    public async Task<IDataResult<UserToListDto>> LoginAsync(LoginDto loginDto)
    {
        var salt = await userRepository.GetUserSaltAsync(loginDto.Email);
        if (string.IsNullOrEmpty(salt))
            return new ErrorDataResult<UserToListDto>(Messages.InvalidUserCredentials.Translate());

        var hashedPassword = passwordHasher.Hash(loginDto.Password, salt);

        var data = await userRepository.GetAsync(m => m.Email == loginDto.Email && m.Password == hashedPassword);
        if (data == null)
            return new ErrorDataResult<UserToListDto>(Messages.InvalidUserCredentials.Translate());

        return new SuccessDataResult<UserToListDto>(mapper.Map<UserToListDto>(data),
            Messages.Success.Translate());
    }

    public async Task<IDataResult<UserToListDto>> LoginByTokenAsync()
    {
        var userId = jwtService.GetUserIdFromToken();
        if (userId is null)
            return new ErrorDataResult<UserToListDto>(Messages.CanNotFoundUserIdInYourAccessToken.Translate());

        var data = await userRepository.GetAsync(m => m.Id == userId);
        if (data == null)
            return new ErrorDataResult<UserToListDto>(Messages.InvalidUserCredentials.Translate());

        return new SuccessDataResult<UserToListDto>(mapper.Map<UserToListDto>(data), Messages.Success.Translate());
    }

    public async Task<IResult> LogoutAsync(string accessToken)
    {
        var tokens = await tokenRepository.GetActiveTokensAsync(accessToken);
        tokens.ForEach(m => m.IsDeleted = true);
        await unitOfWork.CommitAsync();

        foreach (var t in tokens)
            await introspectionCache.MarkRevokedAsync(t.AccessToken, RevokeTtl(t.AccessTokenExpireDate));

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IResult> LogoutRemovedUserAsync(Guid userId)
    {
        var tokens = await tokenRepository.GetListAsync(m => m.UserId == userId);
        tokens.ForEach(m => m.IsDeleted = true);
        await unitOfWork.CommitAsync();

        foreach (var t in tokens)
            await introspectionCache.MarkRevokedAsync(t.AccessToken, RevokeTtl(t.AccessTokenExpireDate));

        return new SuccessResult(Messages.Success.Translate());
    }

    /// <summary>
    ///     Revocation marker only needs to outlive the JWT's <c>exp</c>; after that the signature
    ///     check fails before we ever reach the cache. Add a small grace for clock skew.
    /// </summary>
    private static TimeSpan RevokeTtl(DateTimeOffset accessTokenExpireDate)
    {
        var ttl = accessTokenExpireDate - DateTimeOffset.UtcNow + TimeSpan.FromSeconds(60);
        return ttl > TimeSpan.Zero ? ttl : TimeSpan.Zero;
    }
}