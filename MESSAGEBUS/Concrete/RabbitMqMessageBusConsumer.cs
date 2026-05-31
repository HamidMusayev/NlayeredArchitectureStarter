using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using CORE.Concrete.Observability;
using CORE.Config;
using MESSAGEBUS.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MESSAGEBUS.Concrete;

/// <summary>
///     Companion to <see cref="RabbitMqMessageBus" />. At startup, scans DI for every
///     <c>IMessageHandler&lt;TMessage&gt;</c> registration, declares one durable queue per
///     distinct message type, and binds it to the shared topic exchange. Each consumer
///     deserializes the JSON body and dispatches into a fresh service scope.
///     <para>
///         Messages are <c>ack</c>'d after all handlers succeed, <c>nack</c>'d (with requeue=false)
///         on serialization errors, and <c>nack</c>'d (requeue=true) on handler failures — pair
///         with a dead-letter exchange in production to bound retry loops.
///     </para>
/// </summary>
public sealed class RabbitMqMessageBusConsumer(
    IServiceScopeFactory scopeFactory,
    ConfigSettings config,
    ILogger<RabbitMqMessageBusConsumer> logger) : BackgroundService
{
    private IChannel? _channel;
    private IConnection? _connection;

    private RabbitMqSettings Settings => config.MessageBusSettings.RabbitMq;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscribedTypes = DiscoverSubscribedMessageTypes();
        if (subscribedTypes.Count == 0)
        {
            logger.LogInformation("RabbitMQ consumer started with no IMessageHandler<T> registrations — sleeping.");
            await Task.Delay(Timeout.Infinite, stoppingToken);
            return;
        }

        var factory = new ConnectionFactory
        {
            HostName = Settings.HostName,
            Port = Settings.Port,
            UserName = Settings.UserName,
            Password = Settings.Password,
            VirtualHost = Settings.VirtualHost
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(
            Settings.ExchangeName,
            ExchangeType.Topic,
            true,
            false,
            cancellationToken: stoppingToken);

        foreach (var messageType in subscribedTypes) await BindAndConsumeAsync(messageType, stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task BindAndConsumeAsync(Type messageType, CancellationToken ct)
    {
        var routingKey = messageType.FullName ?? messageType.Name;
        var queueName = $"{Settings.QueuePrefix}{routingKey}";

        await _channel!.QueueDeclareAsync(
            queueName,
            true,
            false,
            false,
            cancellationToken: ct);

        await _channel.QueueBindAsync(
            queueName,
            Settings.ExchangeName,
            routingKey,
            cancellationToken: ct);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            // Per-message activity + correlation restore (mirrors ChannelMessageBusDispatcher).
            // RabbitMQ's BasicProperties.CorrelationId is the standard slot for this.
            using var activity = new Activity("messagebus.dispatch.rabbitmq").Start();
            using var correlationScope = CorrelationContext.Push(args.BasicProperties?.CorrelationId);

            try
            {
                var json = Encoding.UTF8.GetString(args.Body.Span);
                var message = JsonSerializer.Deserialize(json, messageType);
                if (message is null)
                {
                    logger.LogWarning("Discarded null-deserialized RabbitMQ message on {Queue}", queueName);
                    await _channel.BasicNackAsync(args.DeliveryTag, false, false, ct);
                    return;
                }

                await using var scope = scopeFactory.CreateAsyncScope();
                var handlerInterface = typeof(IMessageHandler<>).MakeGenericType(messageType);
                var handlers = scope.ServiceProvider.GetServices(handlerInterface);
                var method = handlerInterface.GetMethod(nameof(IMessageHandler<object>.HandleAsync))!;

                foreach (var handler in handlers)
                {
                    if (handler is null) continue;
                    var task = (Task)method.Invoke(handler, [message, ct])!;
                    await task;
                }

                await _channel.BasicAckAsync(args.DeliveryTag, false, ct);
            }
            catch (JsonException jx)
            {
                logger.LogError(jx, "RabbitMQ message on {Queue} failed JSON deserialization — dropping", queueName);
                await _channel!.BasicNackAsync(args.DeliveryTag, false, false, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Handler dispatch failed for {Queue} — requeueing", queueName);
                await _channel!.BasicNackAsync(args.DeliveryTag, false, true, ct);
            }
        };

        await _channel.BasicConsumeAsync(queueName, false, consumer, ct);
    }

    /// <summary>
    ///     Inspects DI registrations on a throwaway scope to find every distinct <c>TMessage</c>
    ///     closed over <see cref="IMessageHandler{T}" />. Reflection cost paid once at startup.
    /// </summary>
    private HashSet<Type> DiscoverSubscribedMessageTypes()
    {
        using var scope = scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;
        var openHandler = typeof(IMessageHandler<>);

        var types = new HashSet<Type>();
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] asmTypes;
            try
            {
                asmTypes = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                asmTypes = ex.Types.Where(t => t is not null).Cast<Type>().ToArray();
            }

            foreach (var t in asmTypes)
            {
                if (t.IsAbstract || t.IsInterface) continue;
                foreach (var i in t.GetInterfaces())
                {
                    if (!i.IsGenericType || i.GetGenericTypeDefinition() != openHandler) continue;
                    var msgType = i.GetGenericArguments()[0];

                    // Only include when the closed handler interface is actually wired in DI.
                    var closed = openHandler.MakeGenericType(msgType);
                    if (sp.GetServices(closed).Any()) types.Add(msgType);
                }
            }
        }

        return types;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
    }
}