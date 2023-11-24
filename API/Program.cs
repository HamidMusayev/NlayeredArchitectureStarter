using API.Containers;
using API.Filters;
using API.Graphql.Role;
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
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Nummy.CodeLogger.Extensions;
using Nummy.CodeLogger.Models;
using Nummy.ExceptionHandler.Extensions;
using Nummy.ExceptionHandler.Models;
using Nummy.HttpLogger.Extensions;
using Nummy.HttpLogger.Models;
using System.Net;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigSettings();

builder.Configuration.GetSection(nameof(ConfigSettings)).Bind(config);

builder.Services.AddSingleton(config);

builder.Services.AddControllers(opt => opt.Filters.Add(typeof(ModelValidatorActionFilter)))
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblyContaining<ResetPasswordDtoValidator>();

if (config.SentrySettings.IsEnabled) builder.WebHost.UseSentry();

builder.Services.AddAutoMapper(Automapper.GetAutoMapperProfilesFromAllAssemblies().ToArray());

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

builder.Services.AddHealthChecks().AddNpgSql(config.ConnectionStrings.AppDb);

builder.Services.RegisterAuthentication(config);

builder.Services.AddCors(o => o
    .AddPolicy(Constants.EnableAllCorsName, b => b
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowAnyOrigin()));

//builder.Services.AddScoped<LogActionFilter>();

builder.Services.AddScoped<ModelValidatorActionFilter>();

builder.Services.AddEndpointsApiExplorer();

if (config.SwaggerSettings.IsEnabled) builder.Services.RegisterSwagger(config);

builder.Services.RegisterMiniProfiler();

builder.Services.AddSignalR();

builder.Services.AddNummyExceptionHandler(options =>
{
    options.ReturnResponseDuringException = true; // if false, the app throws exceptions as a normal
    options.ResponseContentType = NummyResponseContentType.Json;
    options.ResponseStatusCode = HttpStatusCode.BadRequest;
    options.Response = new ErrorResult(Messages.GeneralError.Translate());
});

builder.Services.AddNummyHttpLogger(options =>
{
    options.EnableRequestLogging = true;
    options.EnableResponseLogging = true;
    options.ExcludeContainingPaths = new[] { "swaggger", "api/user/login", "user/create" };
    options.DatabaseType = NummyHttpLoggerDatabaseType.PostgreSql;
    options.DatabaseConnectionString =
        "Host=localhost;Port=5432;Database=nummy_db;Username=postgres;Password=postgres;IncludeErrorDetail=true;";
});

builder.Services.AddNummyCodeLogger(options =>
{
    options.DatabaseType = NummyCodeLoggerDatabaseType.PostgreSql;
    options.DatabaseConnectionString = 
    "Host=localhost;Port=5432;Database=nummy_db;Username=postgres;Password=postgres;IncludeErrorDetail=true;";
});

//builder.Services.AddAntiforgery();

var app = builder.Build();

// app.UseAntiforgery();

// if (app.Environment.IsDevelopment())

if (config.SwaggerSettings.IsEnabled) app.UseSwagger();

if (config.SwaggerSettings.IsEnabled)
    app.UseSwaggerUI(c => c.InjectStylesheet(config.SwaggerSettings.Theme));

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

app.UseCors(Constants.EnableAllCorsName);

app.UseMiddleware<LocalizationMiddleware>();

app.UseNummyExceptionHandler();
app.UseNummyHttpLogger();

app.UseOutputCache();
app.UseHttpsRedirection();

/*app.Use((context, next) =>
{
    context.Request.EnableBuffering();
    return next();
});*/

// this will cause unexpected behaviour on watchdog's site
/*app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("X-Frame-Options", "Deny");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    await next.Invoke();
});*/

if (config.SentrySettings.IsEnabled) app.UseSentryTracing();

app.UseStaticFiles();

app.UseAuthentication();

// app.UseMiniProfiler();

app.UseRateLimiter();

app.MapHealthChecks(
    "/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.MapControllers();

app.MapHub<UserHub>("/userHub");

app.MapGraphQL((PathString)"/graphql");

app.UseGraphQLVoyager("/graphql-voyager", new VoyagerOptions
{
    GraphQLEndPoint = "/graphql"
});

app.Run();