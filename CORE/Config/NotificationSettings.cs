namespace CORE.Config;

/// <summary>
///     Notification channel feature flags. Lets a deployment disable e.g. push without
///     removing the registration — the channel becomes a no-op when its flag is off.
/// </summary>
public record NotificationSettings
{
    public bool EnableEmail { get; set; } = true;
    public bool EnableSms { get; set; } = true;
    public bool EnablePush { get; set; } = true;
}