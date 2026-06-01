using CORE.Config;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NOTIFICATIONS.Abstract;

namespace NOTIFICATIONS.Concrete;

public class SmtpMailService(IOptions<MailSettings> options) : IMailService
{
    private readonly MailSettings _settings = options.Value;

    public Task SendAsync(string email, string message, CancellationToken ct = default)
    {
        return SendAsync(email, _settings.Subject, message, ct);
    }

    public async Task SendAsync(string email, string subject, string body, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) return;

        var isHtml = body.TrimStart().StartsWith("<", StringComparison.Ordinal);

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_settings.DisplayName, _settings.Address));
        msg.To.Add(MailboxAddress.Parse(email));
        msg.Subject = string.IsNullOrWhiteSpace(subject) ? _settings.Subject : subject;
        msg.Body = new TextPart(isHtml ? "html" : "plain") { Text = body };

        using var client = new SmtpClient();
        var port = int.Parse(_settings.Port);

        var socketOption = _settings.EnableSsl
            ? port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls
            : SecureSocketOptions.None;

        await client.ConnectAsync(_settings.Host, port, socketOption, ct);

        if (!string.IsNullOrEmpty(_settings.MailKey))
            await client.AuthenticateAsync(_settings.Address, _settings.MailKey, ct);

        await client.SendAsync(msg, ct);
        await client.DisconnectAsync(true, ct);
    }
}