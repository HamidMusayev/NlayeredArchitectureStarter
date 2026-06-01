using BLL.Abstract;
using CORE.Abstract;
using CORE.Config;
using CORE.Localization;
using DTO.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace API.Filters;

/// <summary>
///     Authorization filter that checks the application-level token store in addition to the
///     standard JWT signature. Extracts the JWT's <c>jti</c> claim plus the refresh token from
///     the configured request headers, calls <c>ITokenService.CheckValidationAsync</c>, and
///     short-circuits with 401 on failure. Applied via <see cref="ValidateTokenAttribute" />.
/// </summary>
public class ValidateTokenFilter(
    IOptions<AuthSettings> authOptions,
    ITokenService tokenService,
    IJwtService jwtService) : IAsyncAuthorizationFilter
{
    private readonly AuthSettings _auth = authOptions.Value;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata
            .OfType<AllowAnonymousAttribute>().Any();
        if (hasAllowAnonymous) return;

        var jti = jwtService.GetJtiFromToken();
        if (jti is null)
        {
            context.Result = new UnauthorizedObjectResult(
                new ErrorResult(Messages.PermissionDenied.Translate()));
            return;
        }

        string? refreshToken = context.HttpContext.Request
            .Headers[_auth.RefreshTokenHeaderName];

        var validationResult = await tokenService.CheckValidationAsync(jti.Value, refreshToken!);

        if (!validationResult.Success)
            context.Result = new UnauthorizedObjectResult(validationResult);
    }
}