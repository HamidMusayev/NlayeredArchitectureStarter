namespace NOTIFICATIONS.Abstract;

/// <summary>
///     Push-notification surface. Default implementation logs and discards — production deployments
///     register a real impl (Firebase Cloud Messaging, APNs, Expo, etc.) via the same interface.
/// </summary>
public interface IPushService
{
    Task SendAsync(PushRequest request, CancellationToken ct = default);
}

/// <summary>
///     Device-targeted push payload. <paramref name="DeviceTokens" /> carries one or more
///     platform tokens (FCM registration IDs, APNs tokens). <paramref name="Data" /> is a
///     flat string map for client-side parsing.
/// </summary>
public sealed record PushRequest(
    IReadOnlyList<string> DeviceTokens,
    string Title,
    string Body,
    IReadOnlyDictionary<string, string>? Data = null);