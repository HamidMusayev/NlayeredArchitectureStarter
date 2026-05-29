namespace MESSAGEBUS.Abstract;

/// <summary>
///     Handler for a specific message type. Register implementations as <c>Scoped</c> — the
///     bus opens a fresh <see cref="IServiceScope" /> per message so EF Core, the current-user
///     abstraction, and any other request-bound services behave as expected.
///     <para>
///         Multiple handlers may be registered for the same <typeparamref name="TMessage" />;
///         the bus dispatches to all of them.
///     </para>
/// </summary>
public interface IMessageHandler<in TMessage> where TMessage : class
{
    Task HandleAsync(TMessage message, CancellationToken ct);
}