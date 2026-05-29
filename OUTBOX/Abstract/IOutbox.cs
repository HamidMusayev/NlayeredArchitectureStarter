namespace OUTBOX.Abstract;

/// <summary>
///     Outbox enqueue surface used by application services. Adds an <c>OutboxMessage</c>
///     row to the active EF change tracker; the actual <c>SaveChangesAsync</c> is owned by
///     the calling unit of work — keeping the domain write and the outbox row in the
///     same transaction is what makes the pattern reliable.
///     <para>
///         Typical use:
///         <code>
/// await uow.ExecuteInTransactionAsync(async () =>
/// {
///     await userRepo.AddAsync(user);
///     await outbox.EnqueueAsync(new UserCreated(user.Id));
///     return user;
/// });
/// </code>
///     </para>
/// </summary>
public interface IOutbox
{
    Task EnqueueAsync<TMessage>(TMessage message, CancellationToken ct = default) where TMessage : class;
}