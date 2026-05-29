namespace CORE.Config;

/// <summary>
///     Base for any settings block that can be turned off without removing it from
///     configuration. Settings records that inherit this expose an <see cref="IsEnabled" />
///     switch the matching DI registration consults.
/// </summary>
public record Controllable
{
    public bool IsEnabled { get; set; }
}