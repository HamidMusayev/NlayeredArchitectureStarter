using CORE.Config;
using Microsoft.Extensions.Options;
using NOTIFICATIONS.Abstract;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace NOTIFICATIONS.Concrete;

/// <summary>
///     Default <see cref="ISmsService" /> — Twilio Messaging API. Initializes the SDK's static
///     client once per process; <c>SendAsync</c> creates a single <c>MessageResource</c> per
///     call. No-ops gracefully if credentials are blank (lets dev/CI compile and run without
///     configuring Twilio).
/// </summary>
public class TwilioSmsService : ISmsService
{
    private readonly TwilioSettings _settings;

    public TwilioSmsService(IOptions<TwilioSettings> options)
    {
        _settings = options.Value;

        // TwilioClient is a static singleton inside the SDK; initialize once per process.
        if (!string.IsNullOrWhiteSpace(_settings.AccountSid) && !string.IsNullOrWhiteSpace(_settings.AuthToken))
            TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
    }

    public async Task SendAsync(string toPhoneNumber, string message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(toPhoneNumber)) return;

        await MessageResource.CreateAsync(
            new PhoneNumber(toPhoneNumber),
            from: new PhoneNumber(_settings.FromNumber),
            body: message);

        // Twilio SDK ignores CancellationToken on CreateAsync; left here so callers can pass one without surprise.
        ct.ThrowIfCancellationRequested();
    }
}