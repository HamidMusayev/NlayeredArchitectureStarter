using BLL.Abstract;
using CORE.Abstract;
using CORE.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

public class ValidateTokenFilter(
    ConfigSettings configSettings,
    ITokenService tokenService,
    IUtilService utilService) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata
            .OfType<AllowAnonymousAttribute>().Any();
        if (hasAllowAnonymous) return;

        string? jwtToken = context.HttpContext.Request
            .Headers[configSettings.AuthSettings.HeaderName];
        string? refreshToken = context.HttpContext.Request
            .Headers[configSettings.AuthSettings.RefreshTokenHeaderName];

        jwtToken = utilService.TrimToken(jwtToken);

        var validationResult = await tokenService.CheckValidationAsync(jwtToken, refreshToken!);

        if (!validationResult.Success)
            context.Result = new UnauthorizedObjectResult(validationResult);
    }
}
