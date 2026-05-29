using System.Text.Json.Serialization;
using API.Filters;
using Microsoft.AspNetCore.Mvc;

namespace API.Containers.Extensions;

/// <summary>
///     Registers MVC controllers with the <see cref="ModelValidatorActionFilter" /> global filter,
///     JSON enum-string serialization, and suppressed built-in model-state handling.
///     <c>UseControllers</c> maps all controller routes into the pipeline.
/// </summary>
public static class ControllersExtensions
{
    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services.AddControllers(opt => opt.Filters.Add(typeof(ModelValidatorActionFilter)))
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // Suppress [ApiController]'s built-in model state filter so the custom ModelValidatorActionFilter runs instead.
        services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

        services.AddEndpointsApiExplorer();

        return services;
    }

    public static WebApplication UseControllers(this WebApplication app)
    {
        app.MapControllers();
        return app;
    }
}