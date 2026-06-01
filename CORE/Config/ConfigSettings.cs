namespace CORE.Config;

/// <summary>
///     Documentation-only aggregate that lists every per-feature settings record bound under
///     the <c>ConfigSettings</c> root in <c>appsettings.*.json</c>. <b>Not registered in DI</b>
///     and never injected directly — runtime services depend on <see cref="IOptions{T}" /> for
///     the slice they need (e.g. <c>IOptions&lt;AuthSettings&gt;</c>). Bindings are wired up
///     by <c>services.AddConfigOptions(IConfiguration)</c> in <c>CoreServicesExtensions</c>;
///     the startup-time helper <c>IConfiguration.GetConfigSection&lt;T&gt;()</c> covers the
///     few <c>API/Extensions/*.cs</c> branches that inspect a value before any DI scope exists.
///     This type is kept as a single place to discover what's configurable.
/// </summary>
public record ConfigSettings
{
    public AuthSettings AuthSettings { get; set; } = default!;
    public ConnectionStrings ConnectionStrings { get; set; } = default!;
    public RequestSettings RequestSettings { get; set; } = default!;
    public SwaggerSettings SwaggerSettings { get; set; } = default!;
    public RedisSettings RedisSettings { get; set; } = default!;
    public ElasticSearchSettings ElasticSearchSettings { get; set; } = default!;
    public MongoDbSettings MongoDbSettings { get; set; } = default!;
    public ToDoClientSettings ToDoClientSettings { get; set; } = default!;
    public CryptographySettings CryptographySettings { get; set; } = default!;
    public MailSettings MailSettings { get; set; } = default!;
    public TwilioSettings TwilioSettings { get; set; } = default!;
    public LoggingSettings LoggingSettings { get; set; } = new();
    public OpenTelemetrySettings OpenTelemetrySettings { get; set; } = new();
    public MigrationSettings MigrationSettings { get; set; } = new();
    public BlobStorageSettings BlobStorageSettings { get; set; } = new();
    public MessageBusSettings MessageBusSettings { get; set; } = new();
    public CacheSettings CacheSettings { get; set; } = new();
    public NotificationSettings NotificationSettings { get; set; } = new();
    public IdempotencySettings IdempotencySettings { get; set; } = new();
    public FeatureFlagSettings FeatureFlagSettings { get; set; } = new();
    public DistributedLockSettings DistributedLockSettings { get; set; } = new();
    public MultiTenancySettings MultiTenancySettings { get; set; } = new();
    public AuditLogSettings AuditLogSettings { get; set; } = new();
}