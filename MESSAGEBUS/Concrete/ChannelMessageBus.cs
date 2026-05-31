using System.Threading.Channels;
using CORE.Concrete.Observability;
using MESSAGEBUS.Abstract;

namespace MESSAGEBUS.Concrete;

/// <summary>
///     In-process pub/sub backed by <see cref="Channel{T}" />. Default <see cref="IMessageBus" />
///     implementation — zero infrastructure, suitable for monoliths and dev / CI environments.
///     Swap to <c>RabbitMqMessageBus</c> in config when you scale to multiple instances and
///     need cross-process delivery.
///     <para>
///         Producer-side <see cref="PublishAsync" /> writes to the unbounded channel along with
///         the current request's correlation id (captured via <see cref="CorrelationContext.Current" />).
///         A separate hosted service (<c>ChannelMessageBusDispatcher</c>) reads the channel and
///         restores the correlation id before fanning out to handlers, so the publisher's request
///         and the consumer's work share the same id in logs and traces.
///     </para>
/// </summary>
public sealed class ChannelMessageBus : IMessageBus
{
    private readonly Channel<MessageEnvelope> _channel = Channel.CreateUnbounded<MessageEnvelope>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    internal ChannelReader<MessageEnvelope> Reader => _channel.Reader;

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken ct = default) where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(message);
        var envelope = new MessageEnvelope(typeof(TMessage), message, CorrelationContext.Current);
        await _channel.Writer.WriteAsync(envelope, ct);
    }

    public ValueTask PublishCoreAsync(MessageEnvelope envelope, CancellationToken ct)
    {
        return _channel.Writer.WriteAsync(envelope, ct);
    }

    /// <summary>
    ///     Type-tagged carrier so the single-reader dispatcher can resolve handlers without
    ///     reflecting on a boxed object. The runtime type stays available for downstream
    ///     implementations (RabbitMQ serialization etc.) without an extra dictionary lookup.
    ///     <see cref="CorrelationId" /> is the publisher's correlation id captured at enqueue
    ///     time — restored on the consumer side before handlers run.
    /// </summary>
    public sealed record MessageEnvelope(Type MessageType, object Message, string? CorrelationId);
}