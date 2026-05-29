namespace NOTIFICATIONS.Abstract;

/// <summary>SMTP-style mail dispatch surface — default impl is <c>SmtpMailService</c> (MailKit).</summary>
public interface IMailService
{
    /// <summary>
    ///     Sends with a caller-supplied subject. Use this from the composite
    ///     <see cref="INotificationService" /> path where the subject comes from a rendered
    ///     <see cref="IEmailTemplate" />.
    /// </summary>
    Task SendAsync(string email, string subject, string body, CancellationToken ct = default);

    /// <summary>
    ///     Legacy entry-point that pulls the subject from <c>MailSettings.Subject</c>. Kept
    ///     for backwards compatibility with the existing one-line OTP send paths.
    /// </summary>
    Task SendAsync(string email, string message, CancellationToken ct = default);
}