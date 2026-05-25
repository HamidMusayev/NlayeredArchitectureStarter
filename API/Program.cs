using System.Net;
using System.Text.Json.Serialization;
using API.Containers;
using API.Filters;
using API.Graphql.Role;
using API.HangfireJobs;
using API.Hubs;
using API.Middlewares;
using API.Services;
using BLL.Mappers;
using CORE.Config;
using CORE.Constants;
using CORE.Localization;
using DAL.EntityFramework.Context;
using DTO.Auth.Validators;
using DTO.Responses;
using FluentValidation;
using FluentValidation.AspNetCore;
using GraphQL.Server.Ui.Voyager;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nummy.CodeLogger.Extensions;
using Nummy.ExceptionHandler.Extensions;
using Nummy.HealthChecker.Entites;
using Nummy.HealthChecker.Extensions;
using Nummy.HttpLogger.Extensions;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigSettings();

builder.Configuration.GetSection(nameof(ConfigSettings)).Bind(config);

builder.Services.TryAddSingleton(config);

builder.Services.AddControllers(opt => opt.Filters.Add(typeof(ModelValidatorActionFilter)))
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Suppress [ApiController]'s built-in model state filter so our custom ModelValidatorActionFilter runs instead
builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

builder.Services.AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblyContaining<ResetPasswordDtoValidator>();

builder.Services.AddAutoMapper(_ => { },
    Automapper.GetAutoMapperProfilesFromAllAssemblies().ToArray());

builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(config.ConnectionStrings.AppDb));

builder.Services.AddHttpContextAccessor();

builder.Services.RegisterRefitClients(config);

if (config.RedisSettings.IsEnabled)
{
    builder.Services.AddHostedService<RedisIndexCreatorService>();
    builder.Services.RegisterRedis(config);
}

if (config.ElasticSearchSettings.IsEnabled) builder.Services.RegisterElasticSearch(config);
if (config.MongoDbSettings.IsEnabled) builder.Services.RegisterMongoDb();

// configure max request body size as 60 MB
builder.Services.Configure<IISServerOptions>(options => options.MaxRequestBodySize = 60 * 1024 * 1024);

builder.Services.RegisterRepositories();
builder.Services.RegisterSignalRHubs();
builder.Services.RegisterUnitOfWork();
builder.Services.RegisterApiVersioning();
builder.Services.RegisterRateLimit();
builder.Services.RegisterOutputCache();
builder.Services.RegisterMediatr();

builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddProjections()
    .AddSorting()
    .AddFiltering();

builder.Services.RegisterAuthentication(config);

builder.Services.AddCors(o => o
    .AddPolicy(Constants.EnableAllCorsName, b => b
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowAnyOrigin()));

builder.Services.AddScoped<ModelValidatorActionFilter>();

builder.Services.AddEndpointsApiExplorer();

if (config.SwaggerSettings.IsEnabled) builder.Services.RegisterSwagger(config);

builder.Services.RegisterMiniProfiler();

builder.Services.AddSignalR();

builder.Services.AddNummyCodeLogger(options =>
{
    options.NummyServiceUrl = config.NummySettings.ServiceUrl;
    options.ApplicationId = config.NummySettings.ApplicationId;
});

builder.Services.AddNummyHttpLogger(options =>
{
    options.EnableRequestLogging = true;
    options.EnableResponseLogging = true;
    options.ExcludeContainingPaths = ["swagger"];
    options.ApplicationId = config.NummySettings.ApplicationId;
    options.NummyServiceUrl = config.NummySettings.ServiceUrl;
});

builder.Services.AddNummyExceptionHandler(options =>
{
    options.HandleException = true;
    options.ResponseStatusCode = HttpStatusCode.Conflict;
    options.Response = new ErrorResult(Messages.GeneralError.Translate());

    options.ApplicationId = config.NummySettings.ApplicationId;
    options.NummyServiceUrl = config.NummySettings.ServiceUrl;
});

builder.Services.AddNummyHealthChecker(options =>
{
    options.Path = "nummy/health";

    options.CheckAsync = (_, _) => Task.FromResult(new NummyHealthResult
    {
        IsHealthy = true,
        Message = "Service is healthy"
    });
});

// Hangfire
builder.Services.AddHangfire(configuration =>
    configuration.UsePostgreSqlStorage(
        options => options.UseNpgsqlConnection(config.ConnectionStrings.AppDb),
        new PostgreSqlStorageOptions
        {
            SchemaName = "hangfire",
            PrepareSchemaIfNecessary = true,
            QueuePollInterval = TimeSpan.FromSeconds(5)
        }));

builder.Services.AddHangfireServer(options => { options.WorkerCount = Math.Max(1, Environment.ProcessorCount / 2); });

var app = builder.Build();

if (config.SwaggerSettings.IsEnabled) app.UseSwagger();

if (config.SwaggerSettings.IsEnabled)
    app.UseSwaggerUI(c => c.InjectStylesheet(config.SwaggerSettings.Theme));

app.UseCors(Constants.EnableAllCorsName);

app.UseMiddleware<LocalizationMiddleware>();

app.UseNummyExceptionHandler();
app.UseNummyHttpLogger();
app.MapNummyHealthChecker();

app.UseOutputCache();
app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "Deny");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");
    // CSP is intentionally permissive to allow Swagger UI and GraphQL Voyager inline assets
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;");
    await next.Invoke();
});

app.UseHangfireDashboard("/api/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthorizationFilter()]
});

// Recurring: every 30 minutes
RecurringJob.AddOrUpdate<CounterJob>(
    "sample-counter-job",
    job => job.Run(JobCancellationToken.Null),
    "*/30 * * * *",
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Utc
    });

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// app.UseMiniProfiler();

app.UseRateLimiter();

app.MapControllers();

app.MapHub<UserHub>("/userHub");

app.MapGraphQL((PathString)"/graphql");

app.UseGraphQLVoyager("/graphql-voyager", new VoyagerOptions
{
    GraphQLEndPoint = "/graphql"
});

app.Run();
