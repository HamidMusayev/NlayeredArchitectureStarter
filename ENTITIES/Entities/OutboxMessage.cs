using ENTITIES.Entities.Generic;

namespace ENTITIES.Entities;

/// <summary>
///     Durable outgoing-message record written in the same EF transaction as the domain
///     changes that produced it. The <c>OutboxDispatcherHostedService</c> reads pending
///     rows and publishes them via <c>IMessageBus</c>, marking <see cref="ProcessedOnUtc" />
///     only after a successful publish — at-least-once delivery without distributed
///     transactions.
/// </summary>
public class OutboxMessage : Auditable, IEntity
{
    /// <summary>
    ///     Fully-qualified CLR type name of the message body. The dispatcher uses this to
    ///     resolve the original message type when handing off to <c>IMessageBus.PublishAsync</c>.
    /// </summary>
    public required string Type { get; set; }

    /// <summary>JSON-serialized message body.</summary>
    public required string Payload { get; set; }

    /// <summary>
    ///     Correlation id captured from the originating request at enqueue time. The dispatcher
    ///     restores it before invoking handlers so log lines and outbound HTTP calls inside the
    ///     handler share the same id as the request that produced the message. Null = the row
    ///     was enqueued from a context without a correlation id (e.g. a background job).
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>When the originating transaction occurred (UTC).</summary>
    public DateTimeOffset OccurredOnUtc { get; set; }

    /// <summary>When the dispatcher successfully published the message. Null while pending.</summary>
    public DateTimeOffset? ProcessedOnUtc { get; set; }

    /// <summary>Last error message if the dispatcher tried and failed. Null on success.</summary>
    public string? Error { get; set; }

    /// <summary>Number of attempted publishes. Bumped on every dispatcher tick that touches the row.</summary>
    public int AttemptCount { get; set; }

    /// <summary>UTC of the most recent dispatcher attempt — null until the dispatcher has touched the row.</summary>
    public DateTimeOffset? LastAttemptedAt { get; set; }

    /// <summary>
    ///     UTC the row was dead-lettered. Set by the dispatcher once <see cref="AttemptCount" />
    ///     crosses <c>MessageBusSettings.OutboxMaxAttempts</c>; the dispatcher then ignores the
    ///     row. Clear it (via the admin endpoint or a manual UPDATE) to resurrect.
    /// </summary>
    public DateTimeOffset? DeadLetteredAt { get; set; }
}