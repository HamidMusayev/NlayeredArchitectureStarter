using CORE.Config;
using Microsoft.OpenApi;

namespace API.Containers.Extensions;

/// <summary>
///     Registers Swashbuckle with JWT bearer and refresh-token security definitions when
///     <c>SwaggerSettings.IsEnabled</c> is <c>true</c>. <c>UseSwaggerDocumentation</c> serves the
///     JSON spec and the UI with the configured custom theme stylesheet.
/// </summary>
public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, ConfigSettings config)
    {
        if (!config.SwaggerSettings.IsEnabled) return services;

        services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();

            c.SwaggerDoc(config.SwaggerSettings.Version,
                new OpenApiInfo { Title = config.SwaggerSettings.Title, Version = config.SwaggerSettings.Version });

            c.AddSecurityDefinition(config.AuthSettings.TokenPrefix, new OpenApiSecurityScheme
            {
                Name = config.AuthSettings.HeaderName,
                Type = SecuritySchemeType.ApiKey,
                Scheme = config.AuthSettings.TokenPrefix,
                BearerFormat = config.AuthSettings.Type,
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme."
            });

            c.AddSecurityDefinition(config.AuthSettings.RefreshTokenHeaderName, new OpenApiSecurityScheme
            {
                Name = config.AuthSettings.RefreshTokenHeaderName,
                In = ParameterLocation.Header,
                Description = "Refresh token header."
            });

            c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference(config.AuthSettings.TokenPrefix), [] },
                { new OpenApiSecuritySchemeReference(config.AuthSettings.RefreshTokenHeaderName), [] }
            });
        });

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app, ConfigSettings config)
    {
        if (!config.SwaggerSettings.IsEnabled) return app;
        app.UseSwagger();
        app.UseSwaggerUI(c => c.InjectStylesheet(config.SwaggerSettings.Theme));
        return app;
    }
}