namespace CORE.Config;

/// <summary>
///     Selects the active <c>IMessageBus</c> implementation and supplies per-provider settings.
///     Default is <see cref="MessageBusProvider.Channel" /> — in-process, zero infrastructure.
/// </summary>
public record MessageBusSettings
{
    public MessageBusProvider Provider { get; set; } = MessageBusProvider.Channel;

    /// <summary>
    ///     Outbox dispatcher poll interval. Lower = quicker delivery, higher = less DB load.
    /// </summary>
    public int OutboxPollIntervalSeconds { get; set; } = 5;

    /// <summary>
    ///     Max outbox rows processed per dispatcher tick.
    /// </summary>
    public int OutboxBatchSize { get; set; } = 50;

    public RabbitMqSettings RabbitMq { get; set; } = new();
}

public enum MessageBusProvider
{
    Channel = 0,
    RabbitMq = 1
}

public record RabbitMqSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    ///     Shared topic exchange — every publish goes here and every subscriber binds to it
    ///     with a routing key matching the message type's full CLR name.
    /// </summary>
    public string ExchangeName { get; set; } = "starter.events";

    /// <summary>
    ///     Prefix prepended to each per-message-type queue. Lets multiple deployments share
    ///     a broker without trampling each other's queues.
    /// </summary>
    public string QueuePrefix { get; set; } = "starter.";
}