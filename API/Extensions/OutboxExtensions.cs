using MESSAGEBUS.Abstract;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OUTBOX.Abstract;
using OUTBOX.Concrete;
using OUTBOX.Hosted;
using OUTBOX.Sample;

namespace API.Extensions;

/// <summary>
///     Registers the transactional Outbox: the <see cref="IOutbox" /> enqueue surface, the
///     <c>OutboxDispatcherHostedService</c> that drains pending rows on a poll loop, and the
///     sample <see cref="UserSignedUpHandler" /> used by <c>OutboxDemoController</c>.
///     <para>
///         Add additional <c>IMessageHandler&lt;T&gt;</c> registrations here (or extend the Scrutor
///         scan to include the <c>OUTBOX</c> assembly) when introducing new outbox-driven messages.
///     </para>
/// </summary>
public static class OutboxExtensions
{
    public static IServiceCollection AddOutbox(this IServiceCollection services)
    {
        services.TryAddScoped<IOutbox, OutboxService>();
        services.AddScoped<IMessageHandler<UserSignedUpMessage>, UserSignedUpHandler>();
        services.AddHostedService<OutboxDispatcherHostedService>();
        return services;
    }
}