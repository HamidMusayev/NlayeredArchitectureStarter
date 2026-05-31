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

    /// <summary>
    ///     Failed publish attempts a row gets before the dispatcher dead-letters it. Default 10.
    ///     Set to 0 (or negative) to disable dead-lettering — bad rows will then retry forever
    ///     and block the queue head behind themselves.
    /// </summary>
    public int OutboxMaxAttempts { get; set; } = 10;

    /// <summary>
    ///     How long successfully-processed outbox rows are kept before <c>OutboxCleanupJob</c>
    ///     bulk-deletes them. Default 30 days. Set to 0 (or negative) to disable the prune —
    ///     useful if you treat the outbox as an audit log of every published event.
    ///     <para>
    ///         Dead-lettered rows are <b>not</b> pruned by this job; they need a human's eyes
    ///         and the admin endpoint to triage.
    ///     </para>
    /// </summary>
    public int OutboxRetentionDays { get; set; } = 30;

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