using API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Bind every per-feature settings record under ConfigSettings into the DI options system.
// Runtime services then depend on IOptions<XSettings> for just the slice they need.
builder.Services.AddConfigOptions(builder.Configuration);

// Host-level observability install (Serilog hooks into the host).
builder.AddSerilogLogging();

builder.Services
    // Web / MVC backbone
    .AddApiControllers()
    .AddValidation()

    // Storage + business
    .AddMultiTenancy()
    .AddEntityFramework(builder.Configuration)
    .AddMongoDb()
    .AddElasticSearch()
    .AddCoreServices()
    .AddBusinessServices()

    // Cross-cutting abstractions (provider-switched via config)
    .AddCaching(builder.Configuration)
    .AddRedisOm(builder.Configuration)
    .AddBlobStorage(builder.Configuration)
    .AddMessageBus(builder.Configuration)
    .AddOutbox()
    .AddBackgroundQueue()
    .AddEmailTemplating()
    .AddNotifications()
    .AddIdempotency(builder.Configuration)
    .AddFeatureFlags(builder.Configuration)
    .AddDistributedLock(builder.Configuration)

    // Networking + cross-cutting middleware setup
    .AddRateLimit()
    .AddCorsPolicy()
    .AddJwtAuthentication(builder.Configuration)
    .AddCoreProblemDetails()
    .AddCoreHealthChecks(builder.Configuration)
    .AddOpenTelemetryObservability(builder.Configuration)

    // API surfaces
    .AddSwaggerDocumentation(builder.Configuration)
    .AddApiVersioningRules()
    .AddRealtimeHub()
    .AddGraphQlSchema()

    // Background + integration
    .AddHangfireJobs(builder.Configuration)
    .AddRefitHttpClients(builder.Configuration)
    .AddMediatrHandlers()
    .AddMiniProfilerTools()
    .AddIisServerLimits();

var app = builder.Build();

// Pending EF migrations (when MigrationSettings.RunOnStartup = true).
app.ApplyPendingMigrationsIfConfigured();

// Pipeline — order matters. Earlier middlewares wrap later ones.
app
    .UseSwaggerDocumentation()
    // Correlation ID + Serilog request log must come before exception handling so error logs
    // carry the ID. Observability bundles both.
    .UseObservability()
    // ProblemDetails-shaped exception handler wraps everything below.
    .UseProblemDetailsExceptions()
    .UseCorsPolicy()
    .UseLocalization()
    .UseSecurityHeaders()
    .MapCoreHealthChecks()
    .UseOutputCachePipeline()
    .UseHttpsRedirection();

app.UseStaticFiles();

app
    .UseJwtAuthentication()
    .UseIdempotency()
    .UseRateLimit()
    .UseHangfireDashboard()
    .UseRealtimeHub()
    .UseGraphQlEndpoints()
    .UseControllers();

app.Run();