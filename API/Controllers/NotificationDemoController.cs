using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NOTIFICATIONS.Abstract;

namespace API.Controllers;

/// <summary>
///     Demonstration endpoints for <see cref="INotificationService" /> and
///     <see cref="IPushService" />. The composite notification service fans out to email / SMS /
///     push based on which channel fields the request carries — set only the ones you want
///     dispatched, leave the rest <c>null</c>.
///     <para>
///         Channel toggles live in <c>NotificationSettings</c> (<c>EnableEmail</c>,
///         <c>EnableSms</c>, <c>EnablePush</c>). With the default stub providers (SMTP without a
///         real server, the no-op <c>NullPushService</c>) calls succeed and the payload shows up
///         in the API logs — swap impls for production.
///     </para>
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class NotificationDemoController(INotificationService notifications, IPushService push)
    : ControllerBase
{
    /// <summary>
    ///     Single-channel email. The composite logs per-channel failures but won't throw — the
    ///     200 here means "accepted for dispatch", not "delivered".
    /// </summary>
    [HttpPost("email")]
    public async Task<IActionResult> SendEmail([FromBody] EmailDemoRequest body, CancellationToken ct)
    {
        await notifications.SendAsync(
            new NotificationRequest(new EmailRecipient(body.To, body.Subject, body.Body)),
            ct);
        return Ok(new { dispatched = "email", body.To });
    }

    /// <summary>
    ///     Single-channel SMS via the configured <c>ISmsService</c> (Twilio by default).
    /// </summary>
    [HttpPost("sms")]
    public async Task<IActionResult> SendSms([FromBody] SmsDemoRequest body, CancellationToken ct)
    {
        await notifications.SendAsync(
            new NotificationRequest(Sms: new SmsRecipient(body.ToPhoneNumber, body.Message)),
            ct);
        return Ok(new { dispatched = "sms", body.ToPhoneNumber });
    }

    /// <summary>
    ///     Single-channel push. Goes through <see cref="IPushService" /> directly so you can see
    ///     the abstraction in isolation — the default <c>NullPushService</c> just logs the
    ///     payload. Register a real FCM / APNs / Expo impl against <c>IPushService</c> in
    ///     production.
    /// </summary>
    [HttpPost("push")]
    public async Task<IActionResult> SendPush([FromBody] PushDemoRequest body, CancellationToken ct)
    {
        await push.SendAsync(
            new PushRequest(body.DeviceTokens, body.Title, body.Body, body.Data),
            ct);
        return Ok(new { dispatched = "push", devices = body.DeviceTokens.Count });
    }

    /// <summary>
    ///     Fan-out: hits every channel the caller populated in one composite call. Per-channel
    ///     failures are logged but don't poison the others.
    /// </summary>
    [HttpPost("broadcast")]
    public async Task<IActionResult> Broadcast([FromBody] BroadcastDemoRequest body, CancellationToken ct)
    {
        var request = new NotificationRequest(
            body.Email is null
                ? null
                : new EmailRecipient(body.Email.To, body.Email.Subject, body.Email.Body),
            body.Sms is null
                ? null
                : new SmsRecipient(body.Sms.ToPhoneNumber, body.Sms.Message),
            body.Push is null
                ? null
                : new PushRequest(body.Push.DeviceTokens, body.Push.Title, body.Push.Body, body.Push.Data));

        await notifications.SendAsync(request, ct);

        return Ok(new
        {
            dispatched = new[]
            {
                body.Email is not null ? "email" : null,
                body.Sms is not null ? "sms" : null,
                body.Push is not null ? "push" : null
            }.Where(x => x is not null)
        });
    }

    public sealed record EmailDemoRequest(string To, string Subject, string Body);

    public sealed record SmsDemoRequest(string ToPhoneNumber, string Message);

    public sealed record PushDemoRequest(
        IReadOnlyList<string> DeviceTokens,
        string Title,
        string Body,
        IReadOnlyDictionary<string, string>? Data = null);

    public sealed record BroadcastDemoRequest(
        EmailDemoRequest? Email = null,
        SmsDemoRequest? Sms = null,
        PushDemoRequest? Push = null);
}