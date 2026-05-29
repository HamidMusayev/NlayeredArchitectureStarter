using CORE.Constants;

namespace API.Extensions;

/// <summary>
///     Registers and applies the permissive <c>EnableAll</c> CORS policy (any origin, method,
///     and header). Switch to a tighter policy for production derivations.
/// </summary>
public static class CorsExtensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(o => o
            .AddPolicy(Constants.EnableAllCorsName, b => b
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyOrigin()));

        return services;
    }

    public static WebApplication UseCorsPolicy(this WebApplication app)
    {
        app.UseCors(Constants.EnableAllCorsName);
        return app;
    }
}