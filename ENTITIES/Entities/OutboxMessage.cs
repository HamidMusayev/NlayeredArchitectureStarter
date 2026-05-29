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

    /// <summary>When the originating transaction occurred (UTC).</summary>
    public DateTimeOffset OccurredOnUtc { get; set; }

    /// <summary>When the dispatcher successfully published the message. Null while pending.</summary>
    public DateTimeOffset? ProcessedOnUtc { get; set; }

    /// <summary>Last error message if the dispatcher tried and failed. Null on success.</summary>
    public string? Error { get; set; }

    /// <summary>Number of attempted publishes. Bumped on failure; lets the dispatcher back off bad rows.</summary>
    public int AttemptCount { get; set; }
}