using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace API.Extensions;

/// <summary>
///     Registers ASP.NET Core API versioning with a default version of 1.0. Supports URL-segment,
///     <c>x-api-version</c> header, and media-type version readers simultaneously.
/// </summary>
public static class ApiVersioningExtensions
{
    public static IServiceCollection AddApiVersioningRules(this IServiceCollection services)
    {
        services.AddApiVersioning(opt =>
        {
            opt.DefaultApiVersion = new ApiVersion(1, 0);
            opt.AssumeDefaultVersionWhenUnspecified = true;
            opt.ReportApiVersions = true;
            opt.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("x-api-version"),
                new MediaTypeApiVersionReader("x-api-version"));
        });
        return services;
    }
}