using DTO.Auth;
using DTO.Responses;

namespace BLL.Abstract;

public interface IAccountRecoveryService
{
    Task<IResult> SendOtpAsync(string email);
    Task<IResult> ResetPasswordAsync(ResetPasswordDto dto);
}