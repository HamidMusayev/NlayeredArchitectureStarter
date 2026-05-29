namespace MESSAGEBUS.Abstract;

/// <summary>
///     Vendor-neutral publish surface for application messages (domain events, integration events,
///     background commands). Implementations ship under <c>MESSAGEBUS.Concrete</c>:
///     <c>ChannelMessageBus</c> (in-process default), <c>RabbitMqMessageBus</c>, and optional
///     Kafka / Azure Service Bus extensions.
///     <para>
///         Pairs with <see cref="IMessageHandler{TMessage}" /> implementations registered in DI —
///         the bus dispatches each published message to every matching handler.
///     </para>
///     <para>
///         For at-least-once delivery guarantees across the database boundary, write the message
///         to the Outbox table (<c>IOutbox</c>) inside the same EF transaction and let the
///         <c>OutboxDispatcherHostedService</c> hand it off to this bus.
///     </para>
/// </summary>
public interface IMessageBus
{
    /// <summary>
    ///     Publish <paramref name="message" />. Fire-and-forget from the caller's perspective —
    ///     handlers run on background workers; the returned Task completes once the message
    ///     is enqueued (or accepted by the broker), not once handlers finish.
    /// </summary>
    Task PublishAsync<TMessage>(TMessage message, CancellationToken ct = default) where TMessage : class;
}