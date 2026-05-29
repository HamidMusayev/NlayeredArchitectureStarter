using Microsoft.Extensions.DependencyInjection.Extensions;
using NOTIFICATIONS.Abstract;
using NOTIFICATIONS.Concrete;

namespace API.Extensions;

/// <summary>
///     Registers <see cref="NOTIFICATIONS.Concrete.RazorLightEmailTemplate" /> as the
///     <see cref="NOTIFICATIONS.Abstract.IEmailTemplate" /> singleton. Provides Razor-rendered email
///     subject + body from strongly-typed model records.
/// </summary>
public static class EmailTemplatingExtensions
{
    public static IServiceCollection AddEmailTemplating(this IServiceCollection services)
    {
        services.TryAddSingleton<IEmailTemplate, RazorLightEmailTemplate>();
        return services;
    }
}