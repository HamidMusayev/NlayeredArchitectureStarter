using API.Hubs;

namespace API.Extensions;

/// <summary>
///     Registers SignalR and maps <see cref="API.Hubs.UserHub" /> to <c>/userHub</c>.
///     Extend with additional hubs as the application grows.
/// </summary>
public static class SignalRExtensions
{
    public static IServiceCollection AddRealtimeHub(this IServiceCollection services)
    {
        services.AddSignalR();
        return services;
    }

    public static WebApplication UseRealtimeHub(this WebApplication app)
    {
        app.MapHub<UserHub>("/userHub");
        return app;
    }
}