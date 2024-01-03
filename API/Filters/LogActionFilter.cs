/*using BLL.Abstract;
using CORE.Abstract;
using CORE.Config;
using DTO.Logging;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

public class LogActionFilter(IUtilService utilService, ILoggingService loggingService, ConfigSettings configSettings)
    : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;

        var traceIdentifier = httpContext.TraceIdentifier;
        var clientIp = httpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
        var uri = httpContext.Request.Host + httpContext.Request.Path;

        var token = string.Empty;
        Guid? userId = null;
        var authHeaderName = configSettings.AuthSettings.HeaderName;

        if (!string.IsNullOrEmpty(httpContext.Request.Headers[authHeaderName]) &&
            httpContext.Request.Headers[authHeaderName].ToString().Length > 7)
        {
            token = httpContext.Request.Headers[authHeaderName].ToString();
            userId = !string.IsNullOrEmpty(token)
                ? utilService.GetUserIdFromToken()
                : null;
        }

        context.HttpContext.Request.Body.Position = 0;
        using var streamReader = new StreamReader(context.HttpContext.Request.Body);
        var bodyContent = await streamReader.ReadToEndAsync();
        context.HttpContext.Request.Body.Position = 0;

        await next();

        var requestLog = new RequestLogDto(traceIdentifier, clientIp!, uri,
            DateTime.Now, bodyContent, httpContext.Request.Method, token, userId,
            new ResponseLogDto(traceIdentifier, DateTime.Now, httpContext.Response.StatusCode.ToString(), token,
                userId));

        await loggingService.AddLogAsync(requestLog);
    }
}*/

