﻿using API.ActionFilters;
using API.Containers;
using API.Graphql.Role;
using API.Hubs;
using API.Services;
using BLL.Mappers;
using BLL.MediatR;
using CORE.Config;
using CORE.Constants;
using CORE.Middlewares.ExceptionHandler;
using CORE.Middlewares.Translation;
using DAL.DatabaseContext;
using DTO.Auth.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using GraphQL.Server.Ui.Voyager;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterNLogger();

var config = new ConfigSettings();

builder.Configuration.GetSection("Config").Bind(config);

builder.Services.AddSingleton(config);

builder.Services.AddControllers(opt => opt.Filters.Add(typeof(ValidatorActionFilter)));

builder.Services.AddFluentValidationAutoValidation().AddValidatorsFromAssemblyContaining<LoginDtoValidator>();

if (config.SentrySettings.IsEnabled) builder.WebHost.UseSentry();

builder.Services.AddAutoMapper(Automapper.GetAutoMapperProfilesFromAllAssemblies().ToArray());

builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(config.ConnectionStrings.AppDb));

builder.Services.AddHttpContextAccessor();

builder.Services.AddMemoryCache();

builder.Services.RegisterHttpClients(config);

builder.Services.AddHostedService<TokenKeeperHostedService>();

if (config.RedisSettings.IsEnabled) builder.Services.AddHostedService<RedisIndexCreatorService>();

if (config.RedisSettings.IsEnabled) builder.Services.RegisterRedis(config);

builder.Services.RegisterRepositories();

builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddProjections()
    .AddSorting()
    .AddFiltering();

builder.Services.AddMediatR(typeof(MediatrAssemblyContainer).Assembly);

builder.Services.AddHealthChecks();

builder.Services.RegisterAuthentication(config);

builder.Services.AddCors(o =>
    o.AddPolicy(Constants.EnableAllCorsName, b => b.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin()));

builder.Services.AddScoped<LogActionFilter>();

builder.Services.AddEndpointsApiExplorer();

if (config.SwaggerSettings.IsEnabled) builder.Services.RegisterSwagger(config);

builder.Services.RegisterMiniProfiler();

builder.Services.AddSignalR();

var app = builder.Build();

// if (app.Environment.IsDevelopment())

if (config.SwaggerSettings.IsEnabled) app.UseSwagger();

if (config.SwaggerSettings.IsEnabled) app.UseSwaggerUI();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

app.UseCors(Constants.EnableAllCorsName);

app.UseMiddleware<LocalizationMiddleware>();

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.Use((context, next) =>
{
    context.Request.EnableBuffering();
    return next();
});

// this headers will broke miniprofiler. inspect in mini profiler
// app.Use(async (context, next) =>
// {
//     context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
//     context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
//     context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
//     context.Response.Headers.Add("X-Frame-Options", "Deny");
//     context.Response.Headers.Add("Referrer-Policy", "no-referrer");
//     await next.Invoke();
// });

if (config.SentrySettings.IsEnabled) app.UseSentryTracing();

app.UseStaticFiles();

app.UseAuthorization();

app.UseAuthentication();

app.UseMiniProfiler();

app.UseHealthChecks("/hc");

app.MapControllers();

app.MapHub<UserHub>("/userHub");

app.MapGraphQL((PathString)"/graphql");

app.UseGraphQLVoyager("/graphql-voyager", new VoyagerOptions
{
    GraphQLEndPoint = "/graphql"
});

app.Run();