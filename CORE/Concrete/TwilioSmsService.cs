using CORE.Abstract;
using CORE.Config;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace CORE.Concrete;

public class TwilioSmsService : ISmsService
{
    private readonly TwilioSettings _settings;

    public TwilioSmsService(ConfigSettings config)
    {
        _settings = config.TwilioSettings;

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