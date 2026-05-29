using StackExchange.Profiling;
using StackExchange.Profiling.SqlFormatters;

namespace API.Containers.Extensions;

/// <summary>
///     Registers MiniProfiler with dark-color-scheme UI, inline SQL formatting, and EF Core
///     integration. The profiler UI is available at <c>/profiler/results-index</c>.
/// </summary>
public static class MiniProfilerExtensions
{
    public static IServiceCollection AddMiniProfilerTools(this IServiceCollection services)
    {
        services.AddMiniProfiler(options =>
        {
            options.RouteBasePath = "/profiler";
            options.ColorScheme = ColorScheme.Dark;
            options.SqlFormatter = new InlineFormatter();
        }).AddEntityFramework();

        return services;
    }
}