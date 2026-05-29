namespace CORE.Abstract;

/// <summary>
///     Lightweight in-process queue for short fire-and-forget work — sending a confirmation
///     email, warming a cache, posting to a webhook. Sits next to Hangfire: reach for Hangfire
///     when you need scheduling, retries, persistence, or the dashboard. Reach for this when
///     you just want the work to happen "soon" and have no problem if it's lost on restart.
///     <para>
///         The captured delegate runs inside a fresh <see cref="IServiceScope" /> so EF Core,
///         <c>ICurrentUser</c>, and other scoped services behave as in a normal request.
///     </para>
/// </summary>
public interface IBackgroundTaskQueue
{
    ValueTask EnqueueAsync(Func<IServiceProvider, CancellationToken, Task> workItem, CancellationToken ct = default);
}