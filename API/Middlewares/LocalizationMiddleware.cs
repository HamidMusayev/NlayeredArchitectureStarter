using System.Globalization;
using CORE.Constants;

namespace API.Middlewares;

public class LocalizationMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        var requestLang = context.Request.Headers[LocalizationConstants.LangHeaderName].ToString();

        var threadLang = requestLang switch
        {
            LocalizationConstants.LangHeaderAz => "az-Latn",
            LocalizationConstants.LangHeaderEn => "en-GB",
            LocalizationConstants.LangHeaderRu => "ru-RU",

            _ => "az-Latn"
        };

        Thread.CurrentThread.CurrentCulture = new CultureInfo(threadLang);
        Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

        context.Items["ClientLang"] = threadLang;
        context.Items["ClientCulture"] = Thread.CurrentThread.CurrentUICulture.Name;

        LocalizationConstants.CurrentLang = requestLang ?? LocalizationConstants.DefaultLang;

        await next.Invoke(context);
    }
}