using BLL.Abstract;
using BLL.Mappers;
using CORE.Abstract;
using CORE.Concrete.Cache;
using CORE.Config;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using DTO.Auth;
using DTO.Responses;
using DTO.User;
using ENTITIES.Entities;

namespace BLL.Concrete;

/// <summary>
///     Default <see cref="ITokenService" /> implementation. Mints JWT + refresh-token pairs
///     (new family per login), validates by JWT <c>jti</c> against the introspection cache,
///     performs refresh-token rotation with reuse detection (burns the family on a replay
///     attack), and handles soft-delete for logout.
///     <para>
///         Per-request validation is fronted by <see cref="ITokenIntrospectionCache" />: the DB
///         is only consulted on cache miss, and every issuance / rotation / revocation pushes a
///         marker into the cache so all replicas see the change immediately. The full JWT never
///         lands in the database — only its <c>jti</c> claim does.
///     </para>
/// </summary>
public class TokenService(
    ConfigSettings configSettings,
    ITokenRepository tokenRepository,
    ITokenIntrospectionCache introspectionCache,
    IUnitOfWork unitOfWork,
    IJwtService jwtService,
    UserMapper userMapper,
    TokenMapper tokenMapper)
    : ITokenService
{
    public async Task<IResult> AddAsync(LoginResponseDto responseDto)
    {
        // External callers handing us a pre-formed DTO can't supply the jti (it lived inside
        // the JWT that was minted elsewhere). Re-mint instead so the persisted row is
        // internally consistent.
        var fresh = jwtService.CreateTokenForUser(responseDto.User, responseDto.AccessTokenExpireDate);

        var data = BuildTokenEntity(responseDto);
        data.Jti = fresh.Jti;

        await tokenRepository.AddAsync(data);
        await unitOfWork.CommitAsync();

        await introspectionCache.MarkValidAsync(fresh.Jti, responseDto.RefreshToken,
            CacheTtl(data.AccessTokenExpireDate));

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IResult> CheckValidationAsync(Guid jti, string refreshToken)
    {
        var state = await introspectionCache.GetAsync(jti);

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
        var token = await tokenRepository.GetForValidationAsync(jti, refreshToken);
        if (token is null)
            return new ErrorResult(Messages.PermissionDenied.Translate());

        await introspectionCache.MarkValidAsync(jti, refreshToken,
            CacheTtl(token.AccessTokenExpireDate));
        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IDataResult<LoginResponseDto>> CreateTokenAsync(UserToListDto listDto)
    {
        // Fresh login → new token family. Rotation reuses the family via RotateAsync.
        var familyId = Guid.NewGuid();
        var (loginResponseDto, jti) = await IssueAsync(listDto, familyId);
        await unitOfWork.CommitAsync();

        await introspectionCache.MarkValidAsync(jti, loginResponseDto.RefreshToken,
            CacheTtl(loginResponseDto.AccessTokenExpireDate));

        return new SuccessDataResult<LoginResponseDto>(loginResponseDto, Messages.Success.Translate());
    }

    public async Task<IDataResult<LoginResponseDto>> RotateAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return new ErrorDataResult<LoginResponseDto>(Messages.PermissionDenied.Translate());

        var existing = await tokenRepository.GetByRefreshTokenHashAsync(
            TokenIntrospectionCache.Hash(refreshToken), ct);
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
                await introspectionCache.MarkRevokedAsync(t.Jti, CacheTtl(t.AccessTokenExpireDate), ct);

            return new ErrorDataResult<LoginResponseDto>(Messages.PermissionDenied.Translate());
        }

        if (existing.RefreshTokenExpireDate <= DateTimeOffset.UtcNow)
            return new ErrorDataResult<LoginResponseDto>(Messages.PermissionDenied.Translate());

        // Happy path: mark the inbound token used, mint a new pair in the same family.
        existing.UsedAt = DateTimeOffset.UtcNow;

        var userDto = userMapper.ToListDto(existing.User);
        var (newResponse, newJti) = await IssueAsync(userDto, existing.FamilyId);

        await unitOfWork.CommitAsync(ct);

        // The old jti is now unusable (UsedAt is set on the row). Revoke its cache entry so the
        // very next request with the old pair gets 401 without a DB round-trip.
        await introspectionCache.MarkRevokedAsync(existing.Jti, CacheTtl(existing.AccessTokenExpireDate), ct);
        await introspectionCache.MarkValidAsync(newJti, newResponse.RefreshToken,
            CacheTtl(newResponse.AccessTokenExpireDate), ct);

        return new SuccessDataResult<LoginResponseDto>(newResponse, Messages.Success.Translate());
    }

    public async Task<IResult> SoftDeleteAsync(Guid id)
    {
        var data = await tokenRepository.GetAsync(m => m.Id == id);
        if (data is null) return new ErrorResult(Messages.DataNotFound.Translate());

        tokenRepository.SoftDelete(data);
        await unitOfWork.CommitAsync();

        await introspectionCache.MarkRevokedAsync(data.Jti, CacheTtl(data.AccessTokenExpireDate));

        return new SuccessResult(Messages.Success.Translate());
    }

    /// <summary>
    ///     Mints the pair and stages an entity row for the given <paramref name="familyId" />.
    ///     Returns the wire DTO plus the freshly-minted <c>jti</c> (which the JWT carries but
    ///     the DTO doesn't expose). Caller is responsible for <c>CommitAsync</c> so rotation
    ///     can roll the existing token's <c>UsedAt</c> update in the same transaction.
    /// </summary>
    private async Task<(LoginResponseDto Dto, Guid Jti)> IssueAsync(UserToListDto user, Guid familyId)
    {
        var now = DateTime.UtcNow;
        var accessTokenExpireDate = now.AddMinutes(configSettings.AuthSettings.AccessTokenLifetimeMinutes);
        // Refresh lifetime is absolute (from now), not "access expire + N" — the latter would
        // tie the two together and defeat the point of long-lived refresh + short-lived access.
        var refreshExpireDate = now.AddMinutes(configSettings.AuthSettings.RefreshTokenLifetimeMinutes);

        var issued = jwtService.CreateTokenForUser(user, accessTokenExpireDate);

        var dto = new LoginResponseDto(
            user,
            issued.Jwt,
            accessTokenExpireDate,
            jwtService.GenerateRefreshToken(),
            refreshExpireDate);

        var entity = BuildTokenEntity(dto);
        entity.FamilyId = familyId;
        entity.Jti = issued.Jti;

        await tokenRepository.AddAsync(entity);
        return (dto, issued.Jti);
    }

    /// <summary>
    ///     Constructs a <see cref="Token" /> from a <see cref="LoginResponseDto" /> via Mapperly.
    ///     The mapper copies the DTO's primitive fields + projects <c>UserId</c> from the nested
    ///     user. <c>Jti</c> is set by the caller (the JWT was just minted with it);
    ///     <c>RefreshTokenHash</c> is computed here so the plaintext refresh token never lands
    ///     on disk.
    /// </summary>
    private Token BuildTokenEntity(LoginResponseDto dto)
    {
        var entity = new Token
        {
            User = null!,
            RefreshTokenHash = TokenIntrospectionCache.Hash(dto.RefreshToken),
            Jti = Guid.Empty
        };
        tokenMapper.UpdateEntity(dto, entity);
        entity.UsedAt = null;
        entity.IsRevoked = false;
        return entity;
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