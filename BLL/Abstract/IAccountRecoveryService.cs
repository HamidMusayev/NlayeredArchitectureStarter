using DTO.Auth;
using DTO.Responses;

namespace BLL.Abstract;

/// <summary>
///     Password-recovery flow: send a one-time verification code to the user's email
///     (<see cref="SendOtpAsync" />), then exchange the code for a new password
///     (<see cref="ResetPasswordAsync" />).
/// </summary>
public interface IAccountRecoveryService
{
    Task<IResult> SendOtpAsync(string email);
    Task<IResult> ResetPasswordAsync(ResetPasswordDto dto);
}