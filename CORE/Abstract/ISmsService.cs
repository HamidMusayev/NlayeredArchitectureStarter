namespace CORE.Abstract;

public interface ISmsService
{
    /// <summary>
    ///     Sends an SMS message to a single E.164-formatted phone number.
    /// </summary>
    /// <param name="toPhoneNumber">Recipient in E.164 format (e.g. "+15551234567").</param>
    /// <param name="message">Body text. Long messages are split into segments by the provider.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendAsync(string toPhoneNumber, string message, CancellationToken ct = default);
}