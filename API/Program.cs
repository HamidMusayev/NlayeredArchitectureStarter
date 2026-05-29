using API.Containers.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Load ConfigSettings once; share via DI singleton + as a local for the fluent registrations below.
var config = builder.Configuration.LoadConfigSettings();
builder.Services.TryAddSingleton(config);

// Host-level observability install (Serilog hooks into the host).
builder.AddSerilogLogging(config);

builder.Services
    // Web / MVC backbone
    .AddApiControllers()
    .AddValidation()
    .AddAutoMapperProfiles()

    // Storage + business
    .AddMultiTenancy(config)
    .AddPersistence(config)
    .AddCoreServices()
    .AddBusinessServices()

    // Cross-cutting abstractions (provider-switched via config)
    .AddCaching(config)
    .AddBlobStorage(config)
    .AddMessageBus(config)
    .AddOutbox()
    .AddBackgroundQueue()
    .AddEmailTemplating()
    .AddNotifications()
    .AddSearch(config)
    .AddIdempotency(config)
    .AddFeatureFlags(config)
    .AddDistributedLock(config)

    // Networking + cross-cutting middleware setup
    .AddRateLimit()
    .AddCorsPolicy()
    .AddJwtAuthentication(config)
    .AddCoreProblemDetails()
    .AddCoreHealthChecks(config)
    .AddOpenTelemetryObservability(config)

    // API surfaces
    .AddSwaggerDocumentation(config)
    .AddApiVersioningRules()
    .AddRealtimeHub()
    .AddGraphQLSchema()

    // Background + integration
    .AddHangfireJobs(config)
    .AddNummyObservability(config)
    .AddRefitHttpClients(config)
    .AddMediatrHandlers()
    .AddMiniProfilerTools()
    .AddIisServerLimits();

var app = builder.Build();

// Pending EF migrations (when MigrationSettings.RunOnStartup = true).
app.ApplyPendingMigrationsIfConfigured(config);

// Pipeline — order matters. Earlier middlewares wrap later ones.
app
    .UseSwaggerDocumentation(config)
    // Correlation ID + Serilog request log must come before exception handling so error logs
    // carry the ID. Observability bundles both.
    .UseObservability(config)
    // ProblemDetails-shaped exception handler wraps everything below.
    .UseProblemDetailsExceptions()
    .UseCorsPolicy()
    .UseLocalization()
    .UseSecurityHeaders()
    .UseNummyObservability()
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
    .UseGraphQLEndpoints()
    .UseControllers();

app.Run();