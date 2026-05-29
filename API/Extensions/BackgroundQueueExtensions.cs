using CORE.Abstract;
using CORE.Concrete.BackgroundQueue;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Extensions;

/// <summary>
///     Registers the in-process <see cref="CORE.Abstract.IBackgroundTaskQueue" /> backed by a
///     <c>Channel&lt;Func&lt;…&gt;&gt;</c> and its <c>BackgroundQueueHostedService</c> consumer.
///     Use for short fire-and-forget work; reach for Hangfire when scheduling or a UI is needed.
/// </summary>
public static class BackgroundQueueExtensions
{
    public static IServiceCollection AddBackgroundQueue(this IServiceCollection services)
    {
        services.TryAddSingleton<ChannelBackgroundTaskQueue>();
        services.TryAddSingleton<IBackgroundTaskQueue>(sp => sp.GetRequiredService<ChannelBackgroundTaskQueue>());
        services.AddHostedService<BackgroundQueueHostedService>();
        return services;
    }
}