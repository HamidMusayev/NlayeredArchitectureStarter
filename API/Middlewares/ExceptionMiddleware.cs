/*using CORE.Config;
using CORE.Localization;
using CORE.Logging;
using DTO.Responses;
using Microsoft.AspNetCore.Http.Features;
using Sentry;
using System.Net;
using System.Text.Json;
using CORE.Abstract;
using BLL.Abstract;
using DTO.ErrorLog;

namespace API.Middlewares;

public class ExceptionMiddleware(RequestDelegate next, ConfigSettings config, IServiceScopeFactory serviceScopeFactory)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong: {ex}");
            await LogErrorAsync(httpContext, ex);
            if (config.SentrySettings.IsEnabled) SentrySdk.CaptureException(ex);

          //  if (_env.IsDevelopment()) throw;
            await HandleExceptionAsync(httpContext);
        }
    }

    private async Task LogErrorAsync(HttpContext httpContext, Exception ex)
    {
        using (IServiceScope scope = serviceScopeFactory.CreateScope())
        {
            IUtilService _utilService = scope.ServiceProvider.GetRequiredService<IUtilService>();
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var response = new ErrorResult(Messages.GeneralError.Translate());
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}*/

