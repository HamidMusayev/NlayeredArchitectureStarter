using BLL.Abstract;
using BLL.Mappers;
using CORE.Abstract;
using CORE.Config;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using DTO.Auth;
using DTO.Responses;
using DTO.User;
using ENTITIES.Identifiers;

namespace BLL.Concrete;

/// <summary>
///     Default <see cref="IAuthService" /> implementation. Validates credentials with PBKDF2
///     comparison (with consecutive-failure lockout — see <see cref="LoginAsync" />), resolves
///     the user from a JWT claim for token-based re-login, and soft-deletes all associated
///     token rows on logout.
/// </summary>
public class AuthService(
    IUserRepository userRepository,
    ITokenRepository tokenRepository,
    ITokenIntrospectionCache introspectionCache,
    IUnitOfWork unitOfWork,
    UserMapper userMapper,
    IJwtService jwtService,
    IPasswordHasher passwordHasher,
    IAuditLog auditLog,
    ConfigSettings configSettings)
    : IAuthService
{
    /// <summary>
    ///     PBKDF2 credential check with consecutive-failure lockout. Every failure returns the
    ///     same generic <c>InvalidUserCredentials</c> message — unknown email, wrong password,
    ///     and "account currently locked" are deliberately indistinguishable to the caller so
    ///     enumeration probes can't tell which case they hit. Each branch records to the audit
    ///     log so security probes are visible even when the response isn't.
    /// </summary>
    public async Task<IDataResult<UserToListDto>> LoginAsync(LoginDto loginDto)
    {
        var user = await userRepository.GetAsync(m => m.Email == loginDto.Email);
        if (user is null)
        {
            await auditLog.LogAsync("auth.login.unknown_email",
                metadata: $"{{\"email\":\"{loginDto.Email}\"}}");
            return new ErrorDataResult<UserToListDto>(Messages.InvalidUserCredentials.Translate());
        }

        // Lockout window in effect — refuse without even checking the password so attempts
        // during the lockout don't extend it indefinitely.
        if (user.LockedUntil is { } locked && locked > DateTimeOffset.UtcNow)
        {
            await auditLog.LogAsync("auth.login.locked",
                new UserId(user.Id),
                "User", user.Id);
            return new ErrorDataResult<UserToListDto>(Messages.InvalidUserCredentials.Translate());
        }

        var hashedPassword = passwordHasher.Hash(loginDto.Password, user.Salt);
        if (user.Password != hashedPassword)
        {
            user.FailedLoginAttempts += 1;

            var max = configSettings.AuthSettings.MaxFailedLoginAttempts;
            if (max > 0 && user.FailedLoginAttempts >= max)
                user.LockedUntil = DateTimeOffset.UtcNow
                    .AddMinutes(configSettings.AuthSettings.LockoutDurationMinutes);

            await unitOfWork.CommitAsync();
            await auditLog.LogAsync("auth.login.failed",
                new UserId(user.Id),
                "User", user.Id,
                $"{{\"attempts\":{user.FailedLoginAttempts}}}");
            return new ErrorDataResult<UserToListDto>(Messages.InvalidUserCredentials.Translate());
        }

        // Successful auth — clear any prior failure state and any expired-but-still-set lockout.
        if (user.FailedLoginAttempts != 0 || user.LockedUntil is not null)
        {
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;
            await unitOfWork.CommitAsync();
        }

        await auditLog.LogAsync("auth.login.success",
            new UserId(user.Id),
            "User", user.Id);

        return new SuccessDataResult<UserToListDto>(userMapper.ToListDto(user),
            Messages.Success.Translate());
    }

    public async Task<IDataResult<UserToListDto>> LoginByTokenAsync()
    {
        var userId = jwtService.GetUserIdFromToken();
        if (userId is null)
            return new ErrorDataResult<UserToListDto>(Messages.CanNotFoundUserIdInYourAccessToken.Translate());

        var rawId = userId.Value.Value;
        var data = await userRepository.GetAsync(m => m.Id == rawId);
        if (data == null)
            return new ErrorDataResult<UserToListDto>(Messages.InvalidUserCredentials.Translate());

        return new SuccessDataResult<UserToListDto>(userMapper.ToListDto(data), Messages.Success.Translate());
    }

    public async Task<IResult> LogoutAsync(Guid jti)
    {
        var tokens = await tokenRepository.GetActiveTokensByJtiAsync(jti);
        tokens.ForEach(m => m.IsDeleted = true);
        await unitOfWork.CommitAsync();

        foreach (var t in tokens)
            await introspectionCache.MarkRevokedAsync(t.Jti, RevokeTtl(t.AccessTokenExpireDate));

        await auditLog.LogAsync("auth.logout");

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IResult> LogoutRemovedUserAsync(Guid userId)
    {
        var typed = new UserId(userId);
        var tokens = await tokenRepository.GetListAsync(m => m.UserId == typed);
        tokens.ForEach(m => m.IsDeleted = true);
        await unitOfWork.CommitAsync();

        foreach (var t in tokens)
            await introspectionCache.MarkRevokedAsync(t.Jti, RevokeTtl(t.AccessTokenExpireDate));

        await auditLog.LogAsync("auth.logout.user_removed",
            targetType: "User", targetId: userId,
            metadata: $"{{\"tokens_revoked\":{tokens.Count}}}");

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