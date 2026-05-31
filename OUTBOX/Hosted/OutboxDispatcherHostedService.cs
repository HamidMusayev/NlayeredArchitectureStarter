using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using CORE.Concrete.Observability;
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

        var maxAttempts = config.MessageBusSettings.OutboxMaxAttempts;

        foreach (var row in pending)
        {
            row.LastAttemptedAt = DateTimeOffset.UtcNow;

            // Start a per-row activity so OTel sees a span for each dispatch, then restore the
            // originating request's correlation id so logs / outbound calls inside the handler
            // share the same id as the request that produced the message.
            using var activity = new Activity("outbox.dispatch").Start();
            using var correlationScope = CorrelationContext.Push(row.CorrelationId);

            try
            {
                var messageType = Type.GetType(row.Type, false);
                if (messageType is null)
                    throw new InvalidOperationException($"Unable to resolve CLR type: {row.Type}");

                var message = JsonSerializer.Deserialize(row.Payload, messageType)
                              ?? throw new InvalidOperationException("JSON deserialization returned null");

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

                if (maxAttempts > 0 && row.AttemptCount >= maxAttempts)
                {
                    row.DeadLetteredAt = DateTimeOffset.UtcNow;
                    logger.LogError(
                        "Outbox row {Id} dead-lettered after {Attempts} attempts. Last error: {Error}",
                        row.Id, row.AttemptCount, row.Error);
                }
            }
        }

        await uow.CommitAsync(ct);
    }
}