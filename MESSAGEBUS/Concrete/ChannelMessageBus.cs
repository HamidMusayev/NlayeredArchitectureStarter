using System.Threading.Channels;
using MESSAGEBUS.Abstract;

namespace MESSAGEBUS.Concrete;

/// <summary>
///     In-process pub/sub backed by <see cref="Channel{T}" />. Default <see cref="IMessageBus" />
///     implementation — zero infrastructure, suitable for monoliths and dev / CI environments.
///     Swap to <c>RabbitMqMessageBus</c> in config when you scale to multiple instances and
///     need cross-process delivery.
///     <para>
///         Producer-side <see cref="PublishAsync" /> writes to the unbounded channel. A separate
///         hosted service (<c>ChannelMessageBusDispatcher</c>) reads the channel and fans out to
///         the matching <see cref="IMessageHandler{TMessage}" /> instances inside a scoped service
///         provider.
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
        var envelope = new MessageEnvelope(typeof(TMessage), message);
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
    /// </summary>
    public sealed record MessageEnvelope(Type MessageType, object Message);
}