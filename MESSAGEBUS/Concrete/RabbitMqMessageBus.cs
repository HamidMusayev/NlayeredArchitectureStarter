using System.Text.Json;
using CORE.Config;
using MESSAGEBUS.Abstract;
using RabbitMQ.Client;

namespace MESSAGEBUS.Concrete;

/// <summary>
///     RabbitMQ-backed publish path. Messages serialized as JSON and published to a topic
///     exchange (<see cref="MessageBusSettings.RabbitMq" />.<c>ExchangeName</c>) with the
///     CLR type's full name as the routing key. The subscribing side lives in
///     <c>RabbitMqMessageBusConsumer</c>.
///     <para>
///         Connection + channel are created lazily and reused across publishes — RabbitMQ
///         connections are heavyweight; channels are cheap but the consumer needs its own.
///     </para>
/// </summary>
public sealed class RabbitMqMessageBus(ConfigSettings config) : IMessageBus, IAsyncDisposable
{
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private IChannel? _channel;
    private IConnection? _connection;

    private RabbitMqSettings Settings => config.MessageBusSettings.RabbitMq;

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
        _initLock.Dispose();
    }

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken ct = default) where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(message);

        await EnsureChannelAsync(ct);

        var routingKey = typeof(TMessage).FullName ?? typeof(TMessage).Name;
        var payload = JsonSerializer.SerializeToUtf8Bytes(message, message.GetType());

        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Type = routingKey
        };

        await _channel!.BasicPublishAsync(
            Settings.ExchangeName,
            routingKey,
            false,
            props,
            payload,
            ct);
    }

    private async Task EnsureChannelAsync(CancellationToken ct)
    {
        if (_channel is { IsOpen: true }) return;
        await _initLock.WaitAsync(ct);
        try
        {
            if (_channel is { IsOpen: true }) return;

            var factory = new ConnectionFactory
            {
                HostName = Settings.HostName,
                Port = Settings.Port,
                UserName = Settings.UserName,
                Password = Settings.Password,
                VirtualHost = Settings.VirtualHost
            };

            _connection = await factory.CreateConnectionAsync(ct);
            _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

            await _channel.ExchangeDeclareAsync(
                Settings.ExchangeName,
                ExchangeType.Topic,
                true,
                false,
                cancellationToken: ct);
        }
        finally
        {
            _initLock.Release();
        }
    }
}