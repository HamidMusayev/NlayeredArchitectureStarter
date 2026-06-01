using CORE.Abstract;
using CORE.Concrete;
using CORE.Config;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NOTIFICATIONS.Abstract;
using NOTIFICATIONS.Concrete;

namespace API.Extensions;

/// <summary>
///     Binds every per-feature settings record under <c>ConfigSettings</c> into the DI options
///     system, so runtime services can depend on the narrow <see cref="IOptions{T}" /> they
///     actually need (e.g. <c>IOptions&lt;AuthSettings&gt;</c>) instead of the whole
///     <see cref="ConfigSettings" /> aggregate. Also registers the cross-cutting "core" services
///     that don't fit a more specialized feature: JWT helper, current-user abstraction,
///     encryption, mail, SMS, pagination context, password hashing.
/// </summary>
public static class CoreServicesExtensions
{
    private const string Root = nameof(ConfigSettings);

    /// <summary>
    ///     Binds every nested settings record under <c>ConfigSettings</c> as a strongly-typed
    ///     options snapshot. Call once during startup; afterwards every consumer pulls
    ///     <see cref="IOptions{T}" /> / <see cref="IOptionsMonitor{T}" /> / <see cref="IOptionsSnapshot{T}" />
    ///     for the slice it needs.
    /// </summary>
    public static IServiceCollection AddConfigOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthSettings>(configuration.GetSection($"{Root}:{nameof(AuthSettings)}"));
        services.Configure<ConnectionStrings>(configuration.GetSection($"{Root}:{nameof(ConnectionStrings)}"));
        services.Configure<RequestSettings>(configuration.GetSection($"{Root}:{nameof(RequestSettings)}"));
        services.Configure<SwaggerSettings>(configuration.GetSection($"{Root}:{nameof(SwaggerSettings)}"));
        services.Configure<RedisSettings>(configuration.GetSection($"{Root}:{nameof(RedisSettings)}"));
        services.Configure<ElasticSearchSettings>(configuration.GetSection($"{Root}:{nameof(ElasticSearchSettings)}"));
        services.Configure<MongoDbSettings>(configuration.GetSection($"{Root}:{nameof(MongoDbSettings)}"));
        services.Configure<ToDoClientSettings>(configuration.GetSection($"{Root}:{nameof(ToDoClientSettings)}"));
        services.Configure<CryptographySettings>(configuration.GetSection($"{Root}:{nameof(CryptographySettings)}"));
        services.Configure<MailSettings>(configuration.GetSection($"{Root}:{nameof(MailSettings)}"));
        services.Configure<TwilioSettings>(configuration.GetSection($"{Root}:{nameof(TwilioSettings)}"));
        services.Configure<LoggingSettings>(configuration.GetSection($"{Root}:{nameof(LoggingSettings)}"));
        services.Configure<OpenTelemetrySettings>(configuration.GetSection($"{Root}:{nameof(OpenTelemetrySettings)}"));
        services.Configure<MigrationSettings>(configuration.GetSection($"{Root}:{nameof(MigrationSettings)}"));
        services.Configure<BlobStorageSettings>(configuration.GetSection($"{Root}:{nameof(BlobStorageSettings)}"));
        services.Configure<MessageBusSettings>(configuration.GetSection($"{Root}:{nameof(MessageBusSettings)}"));
        services.Configure<CacheSettings>(configuration.GetSection($"{Root}:{nameof(CacheSettings)}"));
        services.Configure<NotificationSettings>(configuration.GetSection($"{Root}:{nameof(NotificationSettings)}"));
        services.Configure<IdempotencySettings>(configuration.GetSection($"{Root}:{nameof(IdempotencySettings)}"));
        services.Configure<FeatureFlagSettings>(configuration.GetSection($"{Root}:{nameof(FeatureFlagSettings)}"));
        services.Configure<DistributedLockSettings>(
            configuration.GetSection($"{Root}:{nameof(DistributedLockSettings)}"));
        services.Configure<MultiTenancySettings>(configuration.GetSection($"{Root}:{nameof(MultiTenancySettings)}"));
        services.Configure<AuditLogSettings>(configuration.GetSection($"{Root}:{nameof(AuditLogSettings)}"));

        return services;
    }

    /// <summary>
    ///     Pull a settings slice straight off <see cref="IConfiguration" /> for the rare
    ///     startup-time branch that needs to inspect a value before any DI scope exists (e.g.
    ///     choosing which provider implementation to register). Runtime code should depend on
    ///     <see cref="IOptions{T}" /> instead.
    /// </summary>
    public static T GetConfigSection<T>(this IConfiguration configuration) where T : class
    {
        return configuration.GetSection($"{Root}:{typeof(T).Name}").Get<T>()
               ?? throw new InvalidOperationException(
                   $"Configuration section 'ConfigSettings:{typeof(T).Name}' missing or empty.");
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