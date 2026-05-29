using Microsoft.Extensions.DependencyInjection.Extensions;
using NOTIFICATIONS.Abstract;
using NOTIFICATIONS.Concrete;

namespace API.Extensions;

/// <summary>
///     Registers <see cref="NOTIFICATIONS.Concrete.CompositeNotificationService" /> as the
///     <see cref="NOTIFICATIONS.Abstract.INotificationService" /> and <c>NullPushService</c> as the default
///     <see cref="NOTIFICATIONS.Abstract.IPushService" />. Replace <c>NullPushService</c> with a real FCM or
///     APNs implementation when push notifications are needed.
/// </summary>
public static class NotificationsExtensions
{
    public static IServiceCollection AddNotifications(this IServiceCollection services)
    {
        services.TryAddSingleton<IPushService, NullPushService>();
        services.TryAddScoped<INotificationService, CompositeNotificationService>();
        return services;
    }
}