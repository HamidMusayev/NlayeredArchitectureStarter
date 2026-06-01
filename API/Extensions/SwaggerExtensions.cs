using CORE.Config;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace API.Extensions;

/// <summary>
///     Registers Swashbuckle with JWT bearer and refresh-token security definitions when
///     <c>SwaggerSettings.IsEnabled</c> is <c>true</c>. <c>UseSwaggerDocumentation</c> serves the
///     JSON spec and the UI with the configured custom theme stylesheet.
/// </summary>
public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services,
        IConfiguration configuration)
    {
        var swagger = configuration.GetConfigSection<SwaggerSettings>();
        var auth = configuration.GetConfigSection<AuthSettings>();

        if (!swagger.IsEnabled) return services;

        services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();

            c.SwaggerDoc(swagger.Version,
                new OpenApiInfo { Title = swagger.Title, Version = swagger.Version });

            c.AddSecurityDefinition(auth.TokenPrefix, new OpenApiSecurityScheme
            {
                Name = auth.HeaderName,
                Type = SecuritySchemeType.ApiKey,
                Scheme = auth.TokenPrefix,
                BearerFormat = auth.Type,
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme."
            });

            c.AddSecurityDefinition(auth.RefreshTokenHeaderName, new OpenApiSecurityScheme
            {
                Name = auth.RefreshTokenHeaderName,
                In = ParameterLocation.Header,
                Description = "Refresh token header."
            });

            c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference(auth.TokenPrefix), [] },
                { new OpenApiSecuritySchemeReference(auth.RefreshTokenHeaderName), [] }
            });
        });

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        var swagger = app.Services.GetRequiredService<IOptions<SwaggerSettings>>().Value;
        if (!swagger.IsEnabled) return app;
        app.UseSwagger();
        app.UseSwaggerUI(c => c.InjectStylesheet(swagger.Theme));
        return app;
    }
}