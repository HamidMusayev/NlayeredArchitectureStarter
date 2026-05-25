using System.Text;
using System.Threading.RateLimiting;
using API.Hubs;
using BLL.Concrete;
using CORE.Abstract;
using CORE.Concrete;
using CORE.Config;
using DAL.ElasticSearch;
using DAL.EntityFramework.Concrete;
using DAL.EntityFramework.UnitOfWork;
using DAL.MongoDb;
using DTO.User;
using MediatR;
using MEDIATRS;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Redis.OM;
using Refit;
using REFITS.Clients;
using StackExchange.Profiling;
using StackExchange.Profiling.SqlFormatters;

namespace API.Containers;

public static class DependencyContainer
{
    extension(IServiceCollection services)
    {
        public void RegisterAuthentication(ConfigSettings config)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.AuthSettings.SecretKey)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(60);
                options.Lockout.AllowedForNewUsers = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/auth/login";
                options.LogoutPath = "/auth/logout";
                options.SlidingExpiration = true;

                options.Cookie = new CookieBuilder
                {
                    HttpOnly = true,
                    Name = ".AspNetCore.Security.Cookie",
                    SameSite = SameSiteMode.Lax,
                    SecurePolicy = CookieSecurePolicy.SameAsRequest
                };
            });
        }

        public void RegisterSwagger(ConfigSettings config)
        {
            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();

                c.SwaggerDoc(config.SwaggerSettings.Version,
                    new OpenApiInfo { Title = config.SwaggerSettings.Title, Version = config.SwaggerSettings.Version });

                c.AddSecurityDefinition(config.AuthSettings.TokenPrefix, new OpenApiSecurityScheme
                {
                    Name = config.AuthSettings.HeaderName,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = config.AuthSettings.TokenPrefix,
                    BearerFormat = config.AuthSettings.Type,
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                c.AddSecurityDefinition(config.AuthSettings.RefreshTokenHeaderName, new OpenApiSecurityScheme
                {
                    Name = config.AuthSettings.RefreshTokenHeaderName,
                    In = ParameterLocation.Header,
                    Description = "Refresh token header."
                });

                c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
                {
                    { new OpenApiSecuritySchemeReference(config.AuthSettings.TokenPrefix), [] },
                    { new OpenApiSecuritySchemeReference(config.AuthSettings.RefreshTokenHeaderName), [] }
                });
            });
        }

        public void RegisterApiVersioning()
        {
            services.AddApiVersioning(opt =>
            {
                opt.DefaultApiVersion = new ApiVersion(1, 0);
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.ReportApiVersions = true;
                opt.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("x-api-version"),
                    new MediaTypeApiVersionReader("x-api-version"));
            });
        }

        public void RegisterRateLimit()
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = 429; //default value is 503
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 5,
                            QueueLimit = 2,
                            Window = TimeSpan.FromSeconds(10)
                        }));

                options.OnRejected = (_, _) => new ValueTask();
            });
        }

        public void RegisterRepositories()
        {
            services.TryAddScoped<IUtilService, UtilService>();

            // util service is in the core assembly, therefore we need to register it separately

            services.Scan(scan => scan
                .FromAssemblies(typeof(UserService).Assembly)
                .AddClasses(classes => classes.InNamespaces("BLL.Concrete"))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.Scan(scan => scan
                .FromAssemblies(typeof(UserRepository).Assembly)
                .AddClasses(classes => classes.InNamespaces("DAL.EntityFramework.Concrete"))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        }

        public void RegisterSignalRHubs()
        {
            services.TryAddSingleton<UserHub>();
        }

        public void RegisterUnitOfWork()
        {
            services.TryAddScoped<IUnitOfWork, UnitOfWork>();
        }

        public void RegisterOutputCache()
        {
            services.AddOutputCache(options =>
                options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(2))));
        }

        public void RegisterRedis(ConfigSettings config)
        {
            services.TryAddSingleton(new RedisConnectionProvider(config.RedisSettings.Connection));
        }

        public void RegisterMongoDb()
        {
            services.TryAddSingleton<IMongoDbService, MongoDbService>();
        }

        public void RegisterElasticSearch(ConfigSettings config)
        {
            services.TryAddScoped<IElasticSearchService<UserToListDto>>(_ =>
                new ElasticSearchService<UserToListDto>(
                    config.ElasticSearchSettings.Connection,
                    config.ElasticSearchSettings.DefaultIndex,
                    config.ElasticSearchSettings.Username,
                    config.ElasticSearchSettings.Password
                ));
        }

        public void RegisterMediatr()
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<MediatrAssemblyContainer>());

            services.Scan(scan =>
                scan.FromAssemblyOf<MediatrAssemblyContainer>()
                    .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );
        }

        public void RegisterMiniProfiler()
        {
            services.AddMiniProfiler(options =>
            {
                // All of this is optional. You can simply call .AddMiniProfiler() for all defaults

                // (Optional) Path to use for profiler URLs, default is /mini-profiler-resources
                options.RouteBasePath = "/profiler";

                options.ColorScheme = ColorScheme.Dark;

                // (Optional) Control storage
                // (default is 30 minutes in MemoryCacheStorage)
                // Note: MiniProfiler will not work if a SizeLimit is set on MemoryCache!
                //   See: https://github.com/MiniProfiler/dotnet/issues/501 for details
                //(options.Storage as MemoryCacheStorage)!.CacheDuration = TimeSpan.FromMinutes(60);
                options.SqlFormatter = new InlineFormatter();
            }).AddEntityFramework();
        }

        public void RegisterRefitClients(ConfigSettings config)
        {
            services
                .AddRefitClient<IToDoClient>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(config.ToDoClientSettings.BaseUrl));
            //Add additional IHttpClientBuilder chained methods as required here:
            // .AddHttpMessageHandler<MyHandler>()
            // .SetHandlerLifetime(TimeSpan.FromMinutes(2));
        }
    }
}