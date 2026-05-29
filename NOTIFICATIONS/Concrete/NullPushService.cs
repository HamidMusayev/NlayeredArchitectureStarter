using Microsoft.Extensions.Logging;
using NOTIFICATIONS.Abstract;

namespace NOTIFICATIONS.Concrete;

/// <summary>
///     No-op default for <see cref="IPushService" />. Logs the request and discards it. Lets
///     callers always depend on <c>IPushService</c> without crashing in dev/test environments
///     that don't have FCM / APNs creds. Production deployments swap in a real impl.
/// </summary>
public sealed class NullPushService(ILogger<NullPushService> logger) : IPushService
{
    public Task SendAsync(PushRequest request, CancellationToken ct = default)
    {
        logger.LogInformation(
            "[NullPushService] would push to {DeviceCount} device(s): {Title}",
            request.DeviceTokens.Count, request.Title);
        return Task.CompletedTask;
    }
}