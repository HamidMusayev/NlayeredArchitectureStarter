using System.Text.Json;
using DAL.EntityFramework.Abstract;
using ENTITIES.Entities;
using OUTBOX.Abstract;

namespace OUTBOX.Concrete;

/// <summary>
///     Default <see cref="IOutbox" /> implementation — JSON-serializes the message and stages
///     an <see cref="OutboxMessage" /> via the repository. The caller's
///     <c>IUnitOfWork.CommitAsync()</c> (or <c>ExecuteInTransactionAsync</c>) is what actually
///     flushes the row to the database, so the outbox row and the domain write commit atomically.
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
            OccurredOnUtc = DateTimeOffset.UtcNow
        };

        await repository.AddAsync(entity);
    }
}