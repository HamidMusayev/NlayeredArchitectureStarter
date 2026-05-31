using System.Diagnostics;
using CORE.Concrete.Observability;
using MESSAGEBUS.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MESSAGEBUS.Concrete;

/// <summary>
///     Background reader for <see cref="ChannelMessageBus" />. Pulls envelopes off the channel
///     and dispatches each to every registered <c>IMessageHandler&lt;TMessage&gt;</c> in a
///     fresh service scope. Exceptions thrown by individual handlers are logged but do not
///     poison the channel — the dispatcher keeps consuming.
///     <para>
///         Per envelope a fresh <see cref="Activity" /> is started so OTel sees one span per
///         dispatch, and the publisher's correlation id (carried on the envelope) is restored
///         onto that activity's baggage. Logs / outbound HTTP calls inside the handler then
///         share the originating request's id even though the work runs on a different thread
///         later.
///     </para>
/// </summary>
public sealed class ChannelMessageBusDispatcher(
    ChannelMessageBus bus,
    IServiceScopeFactory scopeFactory,
    ILogger<ChannelMessageBusDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var envelope in bus.Reader.ReadAllAsync(stoppingToken))
        {
            using var activity = new Activity("messagebus.dispatch").Start();
            using var correlationScope = CorrelationContext.Push(envelope.CorrelationId);

            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var handlerInterface = typeof(IMessageHandler<>).MakeGenericType(envelope.MessageType);
                var handlers = scope.ServiceProvider.GetServices(handlerInterface);

                foreach (var handler in handlers)
                {
                    if (handler is null) continue;
                    var method = handlerInterface.GetMethod(nameof(IMessageHandler<object>.HandleAsync))!;
                    var task = (Task)method.Invoke(handler, [envelope.Message, stoppingToken])!;
                    await task;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Host shutdown — exit cleanly.
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Handler dispatch failed for message type {MessageType}",
                    envelope.MessageType.FullName);
            }
        }
    }
}