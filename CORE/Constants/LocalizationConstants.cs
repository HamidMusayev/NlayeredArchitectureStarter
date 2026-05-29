namespace CORE.Constants;

/// <summary>
///     Language-tag constants read by <c>LocalizationMiddleware</c> and the <c>Translate</c>
///     extension. <c>CurrentLang</c> is mutated per request; treat as request-scoped state.
/// </summary>
public class LocalizationConstants
{
    public const string LangHeaderName = "lang";

    public const string LangHeaderAz = "az";
    public const string LangHeaderEn = "en";
    public const string LangHeaderRu = "ru";

    public static string CurrentLang = "az";
    public static string DefaultLang = "az";
}