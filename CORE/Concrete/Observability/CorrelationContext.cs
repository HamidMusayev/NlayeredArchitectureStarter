using System.Diagnostics;

namespace CORE.Concrete.Observability;

/// <summary>
///     Cross-context accessor for the per-request correlation id. The HTTP middleware
///     (<c>CorrelationIdMiddleware</c>) seeds the id onto <see cref="Activity.Current" />'s
///     baggage and into Serilog's <c>LogContext</c>; this helper exposes the same id to layers
///     that don't have an <c>IHttpContextAccessor</c> — outbox dispatchers, message-bus consumers,
///     background-queue workers, hosted services.
///     <para>
///         Baggage rides on <see cref="Activity" />, which uses <see cref="AsyncLocal{T}" />, so
///         a value set in the originating request flows through any awaited work that runs in
///         the same logical execution context. The async boundaries that <b>break</b> that
///         propagation (writing a row to a queue then reading it later on a different thread)
///         are the ones we explicitly capture-then-restore here.
///     </para>
/// </summary>
public static class CorrelationContext
{
    /// <summary>Baggage key. Matches the property name <c>CorrelationIdMiddleware</c> pushes into Serilog's LogContext.</summary>
    public const string Key = "CorrelationId";

    /// <summary>The current id, or <c>null</c> when none has been set on this execution context.</summary>
    public static string? Current => Activity.Current?.GetBaggageItem(Key);

    /// <summary>
    ///     Sets <paramref name="correlationId" /> on the current activity's baggage. Returns an
    ///     <see cref="IDisposable" /> that restores the prior value on dispose (or no-op when no
    ///     prior value existed). Safe to call with <c>null</c> — returns a no-op disposable so
    ///     <c>using</c> blocks don't need to guard.
    /// </summary>
    public static IDisposable Push(string? correlationId)
    {
        if (string.IsNullOrEmpty(correlationId) || Activity.Current is null)
            return NoopDisposable.Instance;

        var previous = Activity.Current.GetBaggageItem(Key);
        Activity.Current.SetBaggage(Key, correlationId);
        return new Scope(previous);
    }

    private sealed class Scope(string? previous) : IDisposable
    {
        public void Dispose()
        {
            Activity.Current?.SetBaggage(Key, previous);
        }
    }

    private sealed class NoopDisposable : IDisposable
    {
        public static readonly NoopDisposable Instance = new();

        public void Dispose()
        {
        }
    }
}