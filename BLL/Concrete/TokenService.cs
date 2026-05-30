using AutoMapper;
using BLL.Abstract;
using CORE.Abstract;
using CORE.Concrete.Cache;
using CORE.Config;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using DTO.Auth;
using DTO.Responses;
using DTO.Token;
using DTO.User;
using ENTITIES.Entities;

namespace BLL.Concrete;

/// <summary>
///     Default <see cref="ITokenService" /> implementation. Mints JWT + refresh-token pairs
///     (new family per login), validates/retrieves tokens, performs refresh-token rotation with
///     reuse detection (burns the family on a replay attack), and handles soft-delete for logout.
///     <para>
///         Per-request validation is fronted by <see cref="ITokenIntrospectionCache" />: the DB
///         is only consulted on cache miss, and every issuance / rotation / revocation pushes a
///         marker into the cache so all replicas see the change immediately.
///     </para>
/// </summary>
public class TokenService(
    ConfigSettings configSettings,
    ITokenRepository tokenRepository,
    ITokenIntrospectionCache introspectionCache,
    IUnitOfWork unitOfWork,
    IJwtService jwtService,
    IMapper mapper)
    : ITokenService
{
    public async Task<IResult> AddAsync(LoginResponseDto responseDto)
    {
        var data = mapper.Map<Token>(responseDto);

        await tokenRepository.AddAsync(data);
        await unitOfWork.CommitAsync();

        await introspectionCache.MarkValidAsync(data.AccessToken, data.RefreshToken,
            CacheTtl(data.AccessTokenExpireDate));

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IDataResult<TokenToListDto>> GetAsync(string accessToken, string refreshToken)
    {
        var token = await tokenRepository.GetAsync(m =>
            m.AccessToken == accessToken && m.RefreshToken == refreshToken &&
            m.RefreshTokenExpireDate > DateTime.UtcNow);
        if (token == null) return new ErrorDataResult<TokenToListDto>(Messages.PermissionDenied.Translate());

        var data = mapper.Map<TokenToListDto>(token);

        return new SuccessDataResult<TokenToListDto>(data, Messages.Success.Translate());
    }

    public async Task<IResult> CheckValidationAsync(string accessToken, string refreshToken)
    {
        var state = await introspectionCache.GetAsync(accessToken);

        switch (state.Status)
        {
            case TokenCacheStatus.Revoked:
                return new ErrorResult(Messages.PermissionDenied.Translate());

            case TokenCacheStatus.Valid:
                // Refresh hash must match the pair we issued. Mismatch = either a forged refresh
                // header or a stale cache entry — fall through to the DB to be sure.
                if (state.RefreshHash == TokenIntrospectionCache.Hash(refreshToken))
                    return new SuccessResult(Messages.Success.Translate());
                break;

            case TokenCacheStatus.Unknown:
            default:
                break;
        }

        // Cache miss (or pair mismatch on a cache hit): authoritative DB lookup, repopulate.
        var token = await tokenRepository.GetForValidationAsync(accessToken, refreshToken);
        if (token is null)
            return new ErrorResult(Messages.PermissionDenied.Translate());

        await introspectionCache.MarkValidAsync(accessToken, refreshToken,
            CacheTtl(token.AccessTokenExpireDate));
        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IDataResult<LoginResponseDto>> CreateTokenAsync(UserToListDto listDto)
    {
        // Fresh login → new token family. Rotation reuses the family via RotateAsync.
        var familyId = Guid.NewGuid();
        var loginResponseDto = await IssueAsync(listDto, familyId);
        await unitOfWork.CommitAsync();

        await introspectionCache.MarkValidAsync(loginResponseDto.AccessToken, loginResponseDto.RefreshToken,
            CacheTtl(loginResponseDto.AccessTokenExpireDate));

        return new SuccessDataResult<LoginResponseDto>(loginResponseDto, Messages.Success.Translate());
    }

    public async Task<IDataResult<LoginResponseDto>> RotateAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return new ErrorDataResult<LoginResponseDto>(Messages.PermissionDenied.Translate());

        var existing = await tokenRepository.GetByRefreshTokenAsync(refreshToken, ct);
        if (existing is null || existing.IsDeleted)
            return new ErrorDataResult<LoginResponseDto>(Messages.PermissionDenied.Translate());

        if (existing.IsRevoked)
            return new ErrorDataResult<LoginResponseDto>(Messages.PermissionDenied.Translate());

        if (existing.UsedAt is not null)
        {
            // Reuse detection — the rotated-out refresh token is back. Burn the whole family.
            var revoked = await tokenRepository.RevokeFamilyAsync(existing.FamilyId, ct);
            await unitOfWork.CommitAsync(ct);

            foreach (var t in revoked)
                await introspectionCache.MarkRevokedAsync(t.AccessToken, CacheTtl(t.AccessTokenExpireDate), ct);

            return new ErrorDataResult<LoginResponseDto>(Messages.PermissionDenied.Translate());
        }

        if (existing.RefreshTokenExpireDate <= DateTimeOffset.UtcNow)
            return new ErrorDataResult<LoginResponseDto>(Messages.PermissionDenied.Translate());

        // Happy path: mark the inbound token used, mint a new pair in the same family.
        existing.UsedAt = DateTimeOffset.UtcNow;

        var userDto = mapper.Map<UserToListDto>(existing.User);
        var newResponse = await IssueAsync(userDto, existing.FamilyId);

        await unitOfWork.CommitAsync(ct);

        // The old access token is now unusable (UsedAt is set). Revoke its cache entry so the
        // very next request with the old pair gets 401 without a DB round-trip.
        await introspectionCache.MarkRevokedAsync(existing.AccessToken, CacheTtl(existing.AccessTokenExpireDate), ct);
        await introspectionCache.MarkValidAsync(newResponse.AccessToken, newResponse.RefreshToken,
            CacheTtl(newResponse.AccessTokenExpireDate), ct);

        return new SuccessDataResult<LoginResponseDto>(newResponse, Messages.Success.Translate());
    }

    public async Task<IResult> SoftDeleteAsync(Guid id)
    {
        var data = await tokenRepository.GetAsync(m => m.Id == id);
        if (data is null) return new ErrorResult(Messages.DataNotFound.Translate());

        tokenRepository.SoftDelete(data);
        await unitOfWork.CommitAsync();

        await introspectionCache.MarkRevokedAsync(data.AccessToken, CacheTtl(data.AccessTokenExpireDate));

        return new SuccessResult(Messages.Success.Translate());
    }

    /// <summary>
    ///     Mints the pair and stages an entity row for the given <paramref name="familyId" />.
    ///     Caller is responsible for <c>CommitAsync</c> (so rotation can roll the existing
    ///     token's UsedAt update in the same transaction).
    /// </summary>
    private async Task<LoginResponseDto> IssueAsync(UserToListDto user, Guid familyId)
    {
        var accessTokenExpireDate = DateTime.UtcNow.AddHours(configSettings.AuthSettings.TokenExpirationTimeInHours);
        var refreshExpireDate =
            accessTokenExpireDate.AddMinutes(configSettings.AuthSettings.RefreshTokenAdditionalMinutes);

        var dto = new LoginResponseDto(
            user,
            jwtService.CreateTokenForUser(user, accessTokenExpireDate),
            accessTokenExpireDate,
            jwtService.GenerateRefreshToken(),
            refreshExpireDate);

        var entity = mapper.Map<Token>(dto);
        entity.FamilyId = familyId;
        entity.UsedAt = null;
        entity.IsRevoked = false;

        await tokenRepository.AddAsync(entity);
        return dto;
    }

    /// <summary>JWT's remaining lifetime plus the configured grace, clamped to ≥ 0.</summary>
    private TimeSpan CacheTtl(DateTimeOffset accessTokenExpireDate)
    {
        var remaining = accessTokenExpireDate - DateTimeOffset.UtcNow;
        var grace = TimeSpan.FromSeconds(configSettings.CacheSettings.TokenCacheGraceSeconds);
        var total = remaining + grace;
        return total > TimeSpan.Zero ? total : TimeSpan.Zero;
    }
}
