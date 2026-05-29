using CORE.Config;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NOTIFICATIONS.Abstract;

namespace NOTIFICATIONS.Concrete;

public class SmtpMailService(ConfigSettings config) : IMailService
{
    public Task SendAsync(string email, string message, CancellationToken ct = default)
    {
        return SendAsync(email, config.MailSettings.Subject, message, ct);
    }

    public async Task SendAsync(string email, string subject, string body, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) return;

        var isHtml = body.TrimStart().StartsWith("<", StringComparison.Ordinal);

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(config.MailSettings.DisplayName, config.MailSettings.Address));
        msg.To.Add(MailboxAddress.Parse(email));
        msg.Subject = string.IsNullOrWhiteSpace(subject) ? config.MailSettings.Subject : subject;
        msg.Body = new TextPart(isHtml ? "html" : "plain") { Text = body };

        using var client = new SmtpClient();
        var port = int.Parse(config.MailSettings.Port);

        var socketOption = config.MailSettings.EnableSsl
            ? port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls
            : SecureSocketOptions.None;

        await client.ConnectAsync(config.MailSettings.Host, port, socketOption, ct);

        if (!string.IsNullOrEmpty(config.MailSettings.MailKey))
            await client.AuthenticateAsync(config.MailSettings.Address, config.MailSettings.MailKey, ct);

        await client.SendAsync(msg, ct);
        await client.DisconnectAsync(true, ct);
    }
}