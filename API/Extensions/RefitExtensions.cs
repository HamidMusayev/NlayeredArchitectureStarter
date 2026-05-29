using API.Http;
using CORE.Config;
using Refit;
using REFITS.ToDo;

namespace API.Extensions;

/// <summary>
///     Registers Refit HTTP clients and wires the
///     <see cref="API.Http.CorrelationIdDelegatingHandler" /> so every outbound request carries the
///     current <c>X-Correlation-Id</c>. Add new <c>IRefitClient</c> registrations here.
/// </summary>
public static class RefitExtensions
{
    public static IServiceCollection AddRefitHttpClients(this IServiceCollection services, ConfigSettings config)
    {
        // Transient handler so every Refit client picks up the per-request HttpContextAccessor scope.
        services.AddTransient<CorrelationIdDelegatingHandler>();

        services
            .AddRefitClient<IToDoClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(config.ToDoClientSettings.BaseUrl))
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

        return services;
    }
}