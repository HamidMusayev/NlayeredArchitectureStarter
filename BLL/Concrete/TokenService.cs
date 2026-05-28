using AutoMapper;
using BLL.Abstract;
using CORE.Abstract;
using CORE.Config;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using DTO.Auth;
using DTO.Responses;
using DTO.Token;
using DTO.User;
using ENTITIES.Entities;

namespace BLL.Concrete;

public class TokenService(
    ConfigSettings configSettings,
    ITokenRepository tokenRepository,
    IUnitOfWork unitOfWork,
    IJwtService jwtService,
    IMapper mapper)
    : ITokenService
{
    public async Task<IResult> AddAsync(LoginResponseDto responseDto)
    {
        var data = mapper.Map<Token>(responseDto);

        await tokenRepository.AddAsync(data);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }

    public async Task<IDataResult<TokenToListDto>> GetAsync(string accessToken, string refreshToken)
    {
        var token = await tokenRepository.GetAsync(m =>
            m.AccessToken == accessToken && m.RefreshToken == refreshToken &&
            m.RefreshTokenExpireDate > DateTime.UtcNow);
        if (token == null) return new ErrorDataResult<TokenToListDto>(Messages.PermissionDenied.Translate());

        var data = mapper.Map<TokenToListDto>(token);

        return new SuccessDataResult<TokenToListDto>(data, Messages.Success.Translate());
    }

    public async Task<IResult> CheckValidationAsync(string accessToken, string refreshToken)
    {
        return await tokenRepository.IsValid(accessToken, refreshToken)
            ? new SuccessResult(Messages.Success.Translate())
            : new ErrorResult(Messages.PermissionDenied.Translate());
    }

    public async Task<IDataResult<LoginResponseDto>> CreateTokenAsync(UserToListDto listDto)
    {
        var accessTokenExpireDate =
            DateTime.UtcNow.AddHours(configSettings.AuthSettings.TokenExpirationTimeInHours);

        var loginResponseDto = new LoginResponseDto(
            listDto,
            jwtService.CreateTokenForUser(listDto, accessTokenExpireDate),
            accessTokenExpireDate,
            jwtService.GenerateRefreshToken(),
            accessTokenExpireDate.AddMinutes(configSettings.AuthSettings.RefreshTokenAdditionalMinutes)
        );

        await AddAsync(loginResponseDto);

        return new SuccessDataResult<LoginResponseDto>(loginResponseDto, Messages.Success.Translate());
    }

    public async Task<IResult> SoftDeleteAsync(Guid id)
    {
        var data = await tokenRepository.GetAsync(m => m.Id == id);
        if (data is null) return new ErrorResult(Messages.DataNotFound.Translate());

        tokenRepository.SoftDelete(data);
        await unitOfWork.CommitAsync();

        return new SuccessResult(Messages.Success.Translate());
    }
}