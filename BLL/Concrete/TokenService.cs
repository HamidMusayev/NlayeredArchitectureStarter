using AutoMapper;
using BLL.Abstract;
using CORE.Abstract;
using CORE.Config;
using CORE.Helpers;
using CORE.Localization;
using DAL.EntityFramework.UnitOfWork;
using DTO.Auth;
using DTO.Responses;
using DTO.Token;
using DTO.User;
using ENTITIES.Entities;

namespace BLL.Concrete;

public class TokenService(
    ConfigSettings configSettings,
    IUnitOfWork unitOfWork,
    IUtilService utilService,
    IMapper mapper)
    : ITokenService
{
    public async Task<IResult> AddAsync(LoginResponseDto dto)
    {
        var data = mapper.Map<Token>(dto);

        await unitOfWork.TokenRepository.AddAsync(data);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IDataResult<TokenToListDto>> GetAsync(string accessToken, string refreshToken)
    {
        var token = await unitOfWork.TokenRepository.GetAsync(m =>
            m.AccessToken == accessToken && m.RefreshToken == refreshToken &&
            m.RefreshTokenExpireDate > DateTime.UtcNow);
        if (token == null) return new ErrorDataResult<TokenToListDto>(Messages.PermissionDenied.Translate());

        var data = mapper.Map<TokenToListDto>(token);

        return new SuccessDataResult<TokenToListDto>(data, Messages.Success.Translate());
    }

    public async Task<IResult> CheckValidationAsync(string accessToken, string refreshToken)
    {
        return await unitOfWork.TokenRepository.IsValid(accessToken, refreshToken)
            ? new SuccessResult(Messages.Success.Translate())
            : new ErrorResult(Messages.PermissionDenied.Translate());
    }

    public async Task<IDataResult<LoginResponseDto>> CreateTokenAsync(UserToListDto dto)
    {
        var securityHelper = new SecurityHelper(configSettings, utilService);
        var accessTokenExpireDate =
            DateTime.UtcNow.AddHours(configSettings.AuthSettings.TokenExpirationTimeInHours);

        var loginResponseDto = new LoginResponseDto(
            dto,
            securityHelper.CreateTokenForUser(dto, accessTokenExpireDate),
            accessTokenExpireDate,
            utilService.GenerateRefreshToken(),
            accessTokenExpireDate.AddMinutes(configSettings.AuthSettings.RefreshTokenAdditionalMinutes)
        );

        await AddAsync(loginResponseDto);

        return new SuccessDataResult<LoginResponseDto>(loginResponseDto, Messages.Success.Translate());
    }

    public async Task<IResult> SoftDeleteAsync(Guid id)
    {
        var data = await unitOfWork.TokenRepository.GetAsync(m => m.TokenId == id);

        unitOfWork.TokenRepository.SoftDelete(data!);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }
}