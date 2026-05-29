using DTO.Auth;
using DTO.Responses;
using DTO.Token;
using DTO.User;

namespace BLL.Abstract;

/// <summary>
///     JWT + refresh-token lifecycle service. Handles token creation (new family on login),
///     rotation (reuse-detection-aware, same family), validation, lookup, and soft-delete.
/// </summary>
public interface ITokenService
{
    Task<IResult> AddAsync(LoginResponseDto responseDto);
    Task<IResult> SoftDeleteAsync(Guid id);
    Task<IDataResult<TokenToListDto>> GetAsync(string accessToken, string refreshToken);
    Task<IResult> CheckValidationAsync(string accessToken, string refreshToken);

    /// <summary>
    ///     Mints a fresh access+refresh pair for the user. Starts a new token family (new <c>FamilyId</c>).
    ///     Used by login and login-by-token paths.
    /// </summary>
    Task<IDataResult<LoginResponseDto>> CreateTokenAsync(UserToListDto listDto);

    /// <summary>
    ///     Refresh-token rotation: validates the incoming refresh token, marks it used, and mints a
    ///     new pair in the same <c>FamilyId</c>. If the refresh token was already used or the family
    ///     is revoked, returns an error and revokes every row in the family — that's the reuse-detection
    ///     signal indicating likely token theft.
    /// </summary>
    Task<IDataResult<LoginResponseDto>> RotateAsync(string refreshToken, CancellationToken ct = default);
}