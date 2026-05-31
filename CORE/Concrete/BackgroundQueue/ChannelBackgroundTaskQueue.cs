using System.Diagnostics;
using System.Threading.Channels;
using CORE.Abstract;
using CORE.Concrete.Observability;

namespace CORE.Concrete.BackgroundQueue;

/// <summary>
///     Unbounded <see cref="Channel{T}" />-backed implementation of
///     <see cref="IBackgroundTaskQueue" />. A single <c>BackgroundQueueHostedService</c> drains
///     it in arrival order, invoking each work item inside a scoped <see cref="IServiceScope" />.
///     <para>
///         <see cref="EnqueueAsync" /> snapshots the current request's correlation id and wraps
///         the caller's work item in a closure that restores it on the consumer side — so logs
///         and outbound HTTP calls from inside the work item carry the originating request's id
///         even though the work runs on a different thread later.
///     </para>
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

        var capturedCorrelationId = CorrelationContext.Current;

        async Task Wrapped(IServiceProvider sp, CancellationToken innerCt)
        {
            using var activity = new Activity("backgroundqueue.work").Start();
            using var correlationScope = CorrelationContext.Push(capturedCorrelationId);
            await workItem(sp, innerCt);
        }

        return _channel.Writer.WriteAsync(Wrapped, ct);
    }
}