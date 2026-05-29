using BLL.Abstract;
using CORE.Abstract;
using CORE.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

/// <summary>
///     Authorization filter that checks the application-level token store in addition to the
///     standard JWT signature. Extracts the access and refresh tokens from the configured request
///     headers, calls <c>ITokenService.CheckValidationAsync</c>, and short-circuits with 401 on
///     failure. Applied via <see cref="ValidateTokenAttribute" />.
/// </summary>
public class ValidateTokenFilter(
    ConfigSettings configSettings,
    ITokenService tokenService,
    IJwtService jwtService) : IAsyncAuthorizationFilter
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

        jwtToken = jwtService.TrimToken(jwtToken);

        var validationResult = await tokenService.CheckValidationAsync(jwtToken, refreshToken!);

        if (!validationResult.Success)
            context.Result = new UnauthorizedObjectResult(validationResult);
    }
}