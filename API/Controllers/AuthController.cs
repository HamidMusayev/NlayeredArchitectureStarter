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

/// <summary>
///     Authentication endpoints: credential-based login, token re-login, logout, refresh-token
///     rotation, and account-recovery flows (send OTP, reset password).
/// </summary>
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

    [SwaggerOperation(Summary = "refresh access token (rotation + reuse detection)")]
    [Produces(typeof(IDataResult<LoginResponseDto>))]
    [HttpGet("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        // Rotation deliberately runs without [ValidateToken]: the access token may already
        // be expired, that's why the caller is refreshing. The refresh token alone gates the flow,
        // and RotateAsync handles reuse detection (revokes the whole family on replay).
        string refreshToken = HttpContext.Request.Headers[configSettings.AuthSettings.RefreshTokenHeaderName]!;

        var rotation = await tokenService.RotateAsync(refreshToken, ct);
        if (!rotation.Success) return Unauthorized(rotation);

        return Ok(rotation);
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