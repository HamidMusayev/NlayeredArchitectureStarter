using System.Text.Json;
using CORE.Concrete.Observability;
using DAL.EntityFramework.Abstract;
using ENTITIES.Entities;
using OUTBOX.Abstract;

namespace OUTBOX.Concrete;

/// <summary>
///     Default <see cref="IOutbox" /> implementation — JSON-serializes the message and stages
///     an <see cref="OutboxMessage" /> via the repository. The caller's
///     <c>IUnitOfWork.CommitAsync()</c> (or <c>ExecuteInTransactionAsync</c>) is what actually
///     flushes the row to the database, so the outbox row and the domain write commit atomically.
///     <para>
///         The current request's correlation id (via <see cref="CorrelationContext.Current" />)
///         is captured onto the row so the dispatcher can restore it before handlers run —
///         logs and outbound HTTP calls from inside the handler share the originating request's
///         id even though they run on a different thread later.
///     </para>
/// </summary>
public sealed class OutboxService(IOutboxRepository repository) : IOutbox
{
    public async Task EnqueueAsync<TMessage>(TMessage message, CancellationToken ct = default) where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(message);

        var entity = new OutboxMessage
        {
            Type = typeof(TMessage).AssemblyQualifiedName ?? typeof(TMessage).FullName ?? typeof(TMessage).Name,
            Payload = JsonSerializer.Serialize(message, message.GetType()),
            CorrelationId = CorrelationContext.Current,
            OccurredOnUtc = DateTimeOffset.UtcNow
        };

        await repository.AddAsync(entity);
    }
}