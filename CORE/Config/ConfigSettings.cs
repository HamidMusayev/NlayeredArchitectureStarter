namespace CORE.Config;

/// <summary>
///     Root configuration aggregate — every per-feature settings record hangs off this one
///     record. Bound from <c>appsettings.json</c> under the <c>ConfigSettings</c> section via
///     <c>IConfiguration.LoadConfigSettings()</c> and registered as a singleton in DI.
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