namespace CORE.Localization;

/// <summary>
///     Extension that resolves a <see cref="Messages" /> enum value to its localized string
///     from the <c>MsgResource</c> .resx files. Current culture is set per-request by
///     <c>LocalizationMiddleware</c>.
/// </summary>
public static class TranslatorExtension
{
    public static string Translate(this Messages message)
    {
        return MsgResource.ResourceManager.GetString(message.ToString())
               ?? throw new ArgumentNullException(
                   $"{message.ToString()} - Key was not found in MessageResource file");
    }
}