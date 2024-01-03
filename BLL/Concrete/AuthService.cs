using AutoMapper;
using BLL.Abstract;
using CORE.Abstract;
using CORE.Helpers;
using CORE.Localization;
using DAL.EntityFramework.UnitOfWork;
using DTO.Auth;
using DTO.Responses;
using DTO.User;

namespace BLL.Concrete;

public class AuthService(IUnitOfWork unitOfWork, IMapper mapper, IUtilService utilService)
    : IAuthService
{
    public async Task<string?> GetUserSaltAsync(string userEmail)
    {
        return await unitOfWork.UserRepository.GetUserSaltAsync(userEmail);
    }

    public async Task<IDataResult<UserToListDto>> LoginAsync(LoginDto dtos)
    {
        var data =
            await unitOfWork.UserRepository.GetAsync(m =>
                m.Email == dtos.Email && m.Password == dtos.Password);
        if (data == null)
            return new ErrorDataResult<UserToListDto>(Messages.InvalidUserCredentials.Translate());

        return new SuccessDataResult<UserToListDto>(mapper.Map<UserToListDto>(data),
            Messages.Success.Translate());
    }

    public async Task<IDataResult<UserToListDto>> LoginByTokenAsync()
    {
        var userId = utilService.GetUserIdFromToken();
        if (userId is null)
            return new ErrorDataResult<UserToListDto>(Messages.CanNotFoundUserIdInYourAccessToken.Translate());

        var data = await unitOfWork.UserRepository.GetAsync(m => m.Id == userId);
        if (data == null)
            return new ErrorDataResult<UserToListDto>(Messages.InvalidUserCredentials.Translate());

        return new SuccessDataResult<UserToListDto>(mapper.Map<UserToListDto>(data), Messages.Success.Translate());
    }

    public IResult SendOtpAsync(string email)
    {
        //TODO SEND MAIL TO EMAIL
        return new SuccessResult(Messages.VerificationCodeSent.Translate());
    }

    public async Task<IResult> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var data = await unitOfWork.UserRepository.GetAsync(m => m.Email == dto.Email);

        if (data is null) return new ErrorResult(Messages.UserIsNotExist.Translate());

        if (data.LastVerificationCode is null ||
            !data.LastVerificationCode.Equals(dto.VerificationCode))
            return new ErrorResult(Messages.InvalidVerificationCode.Translate());

        data.Salt = SecurityHelper.GenerateSalt();
        data.Password = SecurityHelper.HashPassword(dto.Password, data.Salt);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.PasswordResetted.Translate());
    }

    public async Task<IResult> LogoutAsync(string accessToken)
    {
        var tokens = await unitOfWork.TokenRepository.GetActiveTokensAsync(accessToken);

        tokens.ForEach(m => m.IsDeleted = true);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IResult> LogoutRemovedUserAsync(Guid userId)
    {
        var tokens = await unitOfWork.TokenRepository.GetListAsync(m => m.UserId == userId);
        tokens.ForEach(m => m.IsDeleted = true);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }
}