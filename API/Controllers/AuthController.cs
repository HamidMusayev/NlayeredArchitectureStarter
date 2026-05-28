using API.Attributes;
using BLL.Abstract;
using CORE.Abstract;
using CORE.Config;
using CORE.Localization;
using DTO.Auth;
using DTO.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using IResult = DTO.Responses.IResult;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AuthController(
    IAuthService authService,
    IAccountRecoveryService accountRecoveryService,
    ConfigSettings configSettings,
    IJwtService jwtService,
    ITokenService tokenService)
    : ControllerBase
{
    [SwaggerOperation(Summary = "login")]
    [Produces(typeof(IDataResult<LoginResponseDto>))]
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var loginResult = await authService.LoginAsync(request);
        if (!loginResult.Success) return Unauthorized(loginResult);

        var response = await tokenService.CreateTokenAsync(loginResult.Data!);

        return Ok(response);
    }

    [SwaggerOperation(Summary = "send email for reset password")]
    [Produces(typeof(IResult))]
    [HttpGet("otp")]
    [AllowAnonymous]
    public async Task<IActionResult> SendOtp([FromQuery] string email)
    {
        return Ok(await accountRecoveryService.SendOtpAsync(email));
    }

    [SwaggerOperation(Summary = "refesh access token")]
    [Produces(typeof(IDataResult<LoginResponseDto>))]
    [ValidateToken]
    [HttpGet("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var jwtToken =
            jwtService.TrimToken(
                HttpContext.Request.Headers[configSettings.AuthSettings.HeaderName]!);
        string refreshToken = HttpContext.Request.Headers[configSettings.AuthSettings.RefreshTokenHeaderName]!;

        var tokenResponse = await tokenService.GetAsync(jwtToken, refreshToken);
        if (tokenResponse.Success)
        {
            await tokenService.SoftDeleteAsync(tokenResponse.Data!.Id);
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
        var response = await accountRecoveryService.ResetPasswordAsync(request);
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
        var accessToken = jwtService.TrimToken(jwtService.GetTokenString()!);
        var response = await authService.LogoutAsync(accessToken);

        return Ok(response);
    }
}