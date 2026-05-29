using BLL.Abstract;
using CORE.Abstract;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using DTO.Auth;
using DTO.Responses;
using NOTIFICATIONS.Abstract;

namespace BLL.Concrete;

/// <summary>
///     Default <see cref="IAccountRecoveryService" /> implementation. Generates a 6-digit OTP,
///     stores it on the user row, and sends it via <c>IMailService</c>. Password reset verifies
///     the code, re-hashes with a fresh PBKDF2 salt, and clears the stored code.
/// </summary>
public class AccountRecoveryService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IMailService mailService)
    : IAccountRecoveryService
{
    public async Task<IResult> SendOtpAsync(string email)
    {
        var data = await userRepository.GetAsync(m => m.Email == email);
        if (data is null) return new ErrorResult(Messages.UserIsNotExist.Translate());

        data.LastVerificationCode = Random.Shared.Next(100000, 999999).ToString();
        await unitOfWork.CommitAsync();

        await mailService.SendAsync(email, $"Your verification code: {data.LastVerificationCode}");

        return new SuccessResult(Messages.VerificationCodeSent.Translate());
    }

    public async Task<IResult> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var data = await userRepository.GetAsync(m => m.Email == dto.Email);
        if (data is null) return new ErrorResult(Messages.UserIsNotExist.Translate());

        if (data.LastVerificationCode is null ||
            !data.LastVerificationCode.Equals(dto.VerificationCode))
            return new ErrorResult(Messages.InvalidVerificationCode.Translate());

        data.Salt = passwordHasher.GenerateSalt();
        data.Password = passwordHasher.Hash(dto.Password, data.Salt);
        data.LastVerificationCode = null;
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.PasswordResetted.Translate());
    }
}