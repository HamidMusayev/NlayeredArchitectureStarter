namespace CORE.Config;

/// <summary>Twilio Messaging credentials used by <c>TwilioSmsService</c>.</summary>
public record TwilioSettings
{
    /// <summary>
    ///     Twilio Account SID. Found in the Twilio Console dashboard.
    /// </summary>
    public required string AccountSid { get; set; }

    /// <summary>
    ///     Twilio Auth Token. Treat as a secret — keep out of source control.
    /// </summary>
    public required string AuthToken { get; set; }

    /// <summary>
    ///     E.164-formatted sender number (e.g. "+15551234567") or Twilio Messaging Service SID.
    /// </summary>
    public required string FromNumber { get; set; }
}