using System.Reflection;
using System.Text.Json;
using CORE.Config;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using MESSAGEBUS.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OUTBOX.Hosted;

/// <summary>
///     Drains the <c>OutboxMessage</c> table — for each pending row, deserializes the JSON
///     payload back to its original CLR type and publishes via <see cref="IMessageBus" />.
///     On success the row's <c>ProcessedOnUtc</c> is set; on failure the row's
///     <c>AttemptCount</c> and <c>Error</c> are updated and it remains pending for the
///     next tick.
///     <para>
///         Poll interval and batch size come from <see cref="MessageBusSettings" />. Per-tick
///         work runs in a fresh DI scope so EF tracking is isolated.
///     </para>
/// </summary>
public sealed class OutboxDispatcherHostedService(
    IServiceScopeFactory scopeFactory,
    ConfigSettings config,
    ILogger<OutboxDispatcherHostedService> logger) : BackgroundService
{
    private static readonly MethodInfo PublishGenericMethod =
        typeof(IMessageBus).GetMethod(nameof(IMessageBus.PublishAsync))!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var poll = TimeSpan.FromSeconds(Math.Max(1, config.MessageBusSettings.OutboxPollIntervalSeconds));
        var batchSize = Math.Max(1, config.MessageBusSettings.OutboxBatchSize);

        logger.LogInformation("Outbox dispatcher started — poll {Poll}s, batch {Batch}", poll.TotalSeconds, batchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DrainBatchAsync(batchSize, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox tick failed");
            }

            try
            {
                await Task.Delay(poll, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task DrainBatchAsync(int batchSize, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var repo = sp.GetRequiredService<IOutboxRepository>();
        var uow = sp.GetRequiredService<IUnitOfWork>();
        var bus = sp.GetRequiredService<IMessageBus>();

        var pending = await repo.GetPendingAsync(batchSize, ct);
        if (pending.Count == 0) return;

        foreach (var row in pending)
            try
            {
                var messageType = Type.GetType(row.Type, false);
                if (messageType is null)
                {
                    row.Error = $"Unable to resolve CLR type: {row.Type}";
                    row.AttemptCount++;
                    continue;
                }

                var message = JsonSerializer.Deserialize(row.Payload, messageType);
                if (message is null)
                {
                    row.Error = "JSON deserialization returned null";
                    row.AttemptCount++;
                    continue;
                }

                var closed = PublishGenericMethod.MakeGenericMethod(messageType);
                var task = (Task)closed.Invoke(bus, [message, ct])!;
                await task;

                row.ProcessedOnUtc = DateTimeOffset.UtcNow;
                row.Error = null;
                row.AttemptCount++;
            }
            catch (Exception ex)
            {
                row.AttemptCount++;
                row.Error = ex.Message;
                logger.LogWarning(ex, "Outbox row {Id} publish failed (attempt {Attempt})", row.Id, row.AttemptCount);
            }

        await uow.CommitAsync(ct);
    }
}