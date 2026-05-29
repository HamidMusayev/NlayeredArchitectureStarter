using CORE.Abstract;
using CORE.Concrete;
using CORE.Config;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NOTIFICATIONS.Abstract;
using NOTIFICATIONS.Concrete;

namespace API.Containers.Extensions;

/// <summary>
///     Loads <see cref="ConfigSettings" /> from configuration and registers the cross-cutting
///     "core" services that don't fit a more specialized feature: JWT helper, current-user
///     abstraction, encryption, mail, SMS, pagination context, password hashing.
/// </summary>
public static class CoreServicesExtensions
{
    /// <summary>
    ///     Binds the entire <c>ConfigSettings</c> tree from configuration into a single record.
    ///     Call once, then <c>builder.Services.TryAddSingleton(config)</c>.
    /// </summary>
    public static ConfigSettings LoadConfigSettings(this IConfiguration configuration)
    {
        var config = new ConfigSettings();
        configuration.GetSection(nameof(ConfigSettings)).Bind(config);
        return config;
    }

    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.TryAddScoped<IJwtService, JwtService>();
        services.TryAddScoped<ICurrentUser, HttpCurrentUser>();
        services.TryAddScoped<IEncryptionService, AesEncryptionService>();
        services.TryAddScoped<IMailService, SmtpMailService>();
        services.TryAddSingleton<ISmsService, TwilioSmsService>();
        services.TryAddScoped<IPaginationContext, HttpPaginationContext>();
        services.TryAddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        return services;
    }
}