namespace API.Extensions;

/// <summary>
///     IIS server limits — bump max request body size to 60 MB so file uploads work without
///     413 errors when hosted behind IIS / IIS Express.
/// </summary>
public static class IisExtensions
{
    public static IServiceCollection AddIisServerLimits(this IServiceCollection services)
    {
        services.Configure<IISServerOptions>(options => options.MaxRequestBodySize = 60 * 1024 * 1024);
        return services;
    }
}