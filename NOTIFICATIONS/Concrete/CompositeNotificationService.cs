using Microsoft.Extensions.Logging;
using NOTIFICATIONS.Abstract;

namespace NOTIFICATIONS.Concrete;

/// <summary>
///     Default <see cref="INotificationService" /> — fans the inbound
///     <see cref="NotificationRequest" /> out to <see cref="IMailService" />,
///     <see cref="ISmsService" />, and <see cref="IPushService" /> in parallel. Per-channel
///     failures are logged but isolated: an SMTP outage doesn't block the SMS send.
/// </summary>
public sealed class CompositeNotificationService(
    IMailService mail,
    ISmsService sms,
    IPushService push,
    ILogger<CompositeNotificationService> logger) : INotificationService
{
    public async Task SendAsync(NotificationRequest request, CancellationToken ct = default)
    {
        var tasks = new List<Task>(3);
        if (request.Email is { } email)
            tasks.Add(SendChannelAsync("email", () => mail.SendAsync(email.ToAddress, email.Subject, email.Body, ct)));
        if (request.Sms is { } sms2)
            tasks.Add(SendChannelAsync("sms", () => sms.SendAsync(sms2.ToPhoneNumber, sms2.Message, ct)));
        if (request.Push is { } pushReq) tasks.Add(SendChannelAsync("push", () => push.SendAsync(pushReq, ct)));

        if (tasks.Count == 0) return;
        await Task.WhenAll(tasks);
    }

    private async Task SendChannelAsync(string channel, Func<Task> sendOp)
    {
        try
        {
            await sendOp();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Notification channel {Channel} failed", channel);
        }
    }
}