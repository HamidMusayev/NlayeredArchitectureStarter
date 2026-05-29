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
    Task<IResult> LogoutAsync(string accessToken);
    Task<IResult> LogoutRemovedUserAsync(Guid userId);
}