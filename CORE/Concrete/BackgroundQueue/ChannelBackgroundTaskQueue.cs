using System.Threading.Channels;
using CORE.Abstract;

namespace CORE.Concrete.BackgroundQueue;

/// <summary>
///     Unbounded <see cref="Channel{T}" />-backed implementation of
///     <see cref="IBackgroundTaskQueue" />. A single <c>BackgroundQueueHostedService</c> drains
///     it in arrival order, invoking each work item inside a scoped <see cref="IServiceScope" />.
/// </summary>
public sealed class ChannelBackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _channel =
        Channel.CreateUnbounded<Func<IServiceProvider, CancellationToken, Task>>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    internal ChannelReader<Func<IServiceProvider, CancellationToken, Task>> Reader => _channel.Reader;

    public ValueTask EnqueueAsync(Func<IServiceProvider, CancellationToken, Task> workItem,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        return _channel.Writer.WriteAsync(workItem, ct);
    }
}