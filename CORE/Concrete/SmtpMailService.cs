using CORE.Abstract;
using CORE.Config;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CORE.Concrete;

public class SmtpMailService(ConfigSettings config) : IMailService
{
    public async Task SendAsync(string email, string message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) return;

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(config.MailSettings.DisplayName, config.MailSettings.Address));
        msg.To.Add(MailboxAddress.Parse(email));
        msg.Subject = config.MailSettings.Subject;
        msg.Body = new TextPart("plain") { Text = message };

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