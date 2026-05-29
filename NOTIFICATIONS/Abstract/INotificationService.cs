namespace NOTIFICATIONS.Abstract;

/// <summary>
///     One surface for "tell the user". Composes <see cref="IMailService" />,
///     <see cref="ISmsService" />, and <see cref="IPushService" /> — callers say which channels
///     they want hit and the composite dispatches to each independently. Per-channel failures
///     are logged but don't poison the others.
/// </summary>
public interface INotificationService
{
    Task SendAsync(NotificationRequest request, CancellationToken ct = default);
}

/// <summary>
///     Notification dispatch request. Set only the channel fields you want delivered;
///     nulls are skipped.
/// </summary>
public sealed record NotificationRequest(
    EmailRecipient? Email = null,
    SmsRecipient? Sms = null,
    PushRequest? Push = null);

public sealed record EmailRecipient(string ToAddress, string Subject, string Body);

public sealed record SmsRecipient(string ToPhoneNumber, string Message);