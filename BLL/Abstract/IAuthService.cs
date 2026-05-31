using DTO.Auth;
using DTO.Responses;
using DTO.User;

namespace BLL.Abstract;

/// <summary>
///     Authentication entry points: credential-based login, token-based re-login, and logout
///     (both for normal sign-out and for cleaning up a removed user's sessions).
/// </summary>
public interface IAuthService
{
    Task<IDataResult<UserToListDto>> LoginAsync(LoginDto loginDto);
    Task<IDataResult<UserToListDto>> LoginByTokenAsync();

    /// <summary>
    ///     Soft-deletes the token row identified by the JWT's <c>jti</c> claim and pushes a
    ///     revocation marker into the introspection cache so other replicas see it immediately.
    /// </summary>
    Task<IResult> LogoutAsync(Guid jti);

    /// <summary>Same shape but for every active token a user owns — used when an account is deleted.</summary>
    Task<IResult> LogoutRemovedUserAsync(Guid userId);
}