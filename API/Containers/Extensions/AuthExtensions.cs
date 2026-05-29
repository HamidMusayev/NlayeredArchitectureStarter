using System.Text;
using CORE.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Containers.Extensions;

/// <summary>
///     JWT bearer authentication + ASP.NET Identity cookie defaults. Hosts both halves of the
///     auth feature: <see cref="AddJwtAuthentication" /> at startup and
///     <see cref="UseJwtAuthentication" /> in the pipeline (which calls UseAuthentication+UseAuthorization
///     in the right order).
/// </summary>
public static class AuthExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, ConfigSettings config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.AuthSettings.SecretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(60);
            options.Lockout.AllowedForNewUsers = true;
        });

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/auth/login";
            options.LogoutPath = "/auth/logout";
            options.SlidingExpiration = true;

            options.Cookie = new CookieBuilder
            {
                HttpOnly = true,
                Name = ".AspNetCore.Security.Cookie",
                SameSite = SameSiteMode.Lax,
                SecurePolicy = CookieSecurePolicy.SameAsRequest
            };
        });

        return services;
    }

    public static WebApplication UseJwtAuthentication(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}