using API.Attributes;
using BLL.Abstract;
using CORE.Abstract;
using CORE.Config;
using CORE.Helpers;
using CORE.Localization;
using DTO.Auth;
using DTO.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using IResult = DTO.Responses.IResult;

namespace API.Controllers;

[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AuthController(
    IAuthService authService,
    ConfigSettings configSettings,
    IUtilService utilService,
    ITokenService tokenService)
    : Controller
{
    [SwaggerOperation(Summary = "login")]
    [Produces(typeof(IDataResult<LoginResponseDto>))]
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var userSalt = await authService.GetUserSaltAsync(request.Email);

        if (string.IsNullOrEmpty(userSalt))
            return Ok(new ErrorDataResult<Result>(Messages.InvalidUserCredentials.Translate()));

        request = request with { Password = SecurityHelper.HashPassword(request.Password, userSalt) };

        var loginResult = await authService.LoginAsync(request);
        if (!loginResult.Success) return Unauthorized(loginResult);

        var response = await tokenService.CreateTokenAsync(loginResult.Data!);

        return Ok(response);
    }

    [SwaggerOperation(Summary = "send email for reset password")]
    [Produces(typeof(IResult))]
    [HttpGet("otp")]
    [AllowAnonymous]
    public IActionResult SendOtp([FromQuery] string email)
    {
        return Ok(authService.SendOtpAsync(email));
    }

    [SwaggerOperation(Summary = "refesh access token")]
    [Produces(typeof(IDataResult<LoginResponseDto>))]
    [ValidateToken]
    [HttpGet("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var jwtToken =
            utilService.TrimToken(
                HttpContext.Request.Headers[configSettings.AuthSettings.HeaderName]!);
        string refreshToken = HttpContext.Request.Headers[configSettings.AuthSettings.RefreshTokenHeaderName]!;

        var tokenResponse = await tokenService.GetAsync(jwtToken, refreshToken);
        if (tokenResponse.Success)
        {
            await tokenService.SoftDeleteAsync(tokenResponse.Data!.TokenId);
            var response = await tokenService.CreateTokenAsync(tokenResponse.Data.User);
            return Ok(response);
        }

        return Unauthorized();
    }

    [SwaggerOperation(Summary = "reset password")]
    [Produces(typeof(IResult))]
    [HttpPost("password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        var response = await authService.ResetPasswordAsync(request);
        return Ok(response);
    }

    [SwaggerOperation(Summary = "login by token")]
    [Produces(typeof(IDataResult<LoginResponseDto>))]
    [ValidateToken]
    [HttpGet("login/token")]
    public async Task<IActionResult> LoginByToken()
    {
        if (string.IsNullOrEmpty(HttpContext.Request.Headers.Authorization))
            return Unauthorized(new ErrorResult(Messages.CanNotFoundUserIdInYourAccessToken.Translate()));

        var loginByTokenResponse = await authService.LoginByTokenAsync();
        if (!loginByTokenResponse.Success) return BadRequest(loginByTokenResponse.Data);

        var response = await tokenService.CreateTokenAsync(loginByTokenResponse.Data!);

        return Ok(response);
    }

    [SwaggerOperation(Summary = "logout")]
    [Produces(typeof(IResult))]
    [HttpPost("logout")]
    [ValidateToken]
    public async Task<IActionResult> Logout()
    {
        var accessToken = utilService.TrimToken(utilService.GetTokenString()!);
        var response = await authService.LogoutAsync(accessToken);

        return Ok(response);
    }
}