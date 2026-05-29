using API.Middlewares;

namespace API.Containers.Extensions;

/// <summary>
///     Wires the <see cref="API.Middlewares.LocalizationMiddleware" /> into the request pipeline.
///     The middleware reads the <c>lang</c> header and sets the thread culture for the remainder
///     of the request.
/// </summary>
public static class LocalizationExtensions
{
    public static WebApplication UseLocalization(this WebApplication app)
    {
        app.UseMiddleware<LocalizationMiddleware>();
        return app;
    }
}