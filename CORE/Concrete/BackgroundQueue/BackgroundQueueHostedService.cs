using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CORE.Concrete.BackgroundQueue;

/// <summary>
///     Drains <see cref="ChannelBackgroundTaskQueue" />. Each work item runs in its own DI
///     scope so scoped services (EF DbContext, <c>ICurrentUser</c>, etc.) behave the same
///     way as in a normal request — but failures are isolated: an exception in one item
///     logs and the consumer continues with the next.
/// </summary>
public sealed class BackgroundQueueHostedService(
    ChannelBackgroundTaskQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<BackgroundQueueHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var workItem in queue.Reader.ReadAllAsync(stoppingToken))
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                await workItem(scope.ServiceProvider, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background task work item failed");
            }
    }
}