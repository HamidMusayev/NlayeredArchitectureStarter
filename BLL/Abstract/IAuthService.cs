using DTO.Auth;
using DTO.Responses;
using DTO.User;

namespace BLL.Abstract;

public interface IAuthService
{
    Task<IDataResult<UserToListDto>> LoginAsync(LoginDto loginDto);
    Task<IDataResult<UserToListDto>> LoginByTokenAsync();
    Task<IResult> LogoutAsync(string accessToken);
    Task<IResult> LogoutRemovedUserAsync(Guid userId);
}