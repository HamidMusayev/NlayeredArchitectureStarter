using System.Collections.Concurrent;
using CORE.Abstract;

namespace CORE.Concrete.Locks;

/// <summary>
///     Single-process <see cref="IDistributedLock" />. <b>NOT</b> safe across replicas — uses an
///     in-memory <see cref="ConcurrentDictionary{TKey,TValue}" /> of <see cref="SemaphoreSlim" />
///     keyed by resource name.
///     <para>
///         Default for new projects so dev/CI works with zero infrastructure. Flip
///         <c>DistributedLockSettings.Provider</c> to <c>Redis</c> or <c>PostgresAdvisory</c> for any
///         deployment with more than one instance.
///     </para>
/// </summary>
public sealed class InMemoryDistributedLock : IDistributedLock
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Gates = new();

    public async Task<ILockHandle?> AcquireAsync(
        string resource,
        TimeSpan? wait = null,
        TimeSpan? lifetime = null,
        CancellationToken ct = default)
    {
        var gate = Gates.GetOrAdd(resource, _ => new SemaphoreSlim(1, 1));
        var acquired = await gate.WaitAsync(wait ?? TimeSpan.Zero, ct);
        if (!acquired) return null;

        return new Handle(resource, gate);
    }

    private sealed class Handle(string resource, SemaphoreSlim gate) : ILockHandle
    {
        public string Resource => resource;

        public ValueTask DisposeAsync()
        {
            gate.Release();
            return ValueTask.CompletedTask;
        }
    }
}