using CORE.Config;
using MESSAGEBUS.Abstract;
using MESSAGEBUS.Concrete;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Extensions;

/// <summary>
///     Registers the <see cref="IMessageBus" /> implementation chosen by
///     <c>MessageBusSettings.Provider</c>: <c>Channel</c> (default, in-process) or
///     <c>RabbitMq</c>. The Outbox dispatcher lives in <c>OutboxExtensions</c> so the message
///     bus and the outbox can be enabled independently.
/// </summary>
public static class MessageBusExtensions
{
    public static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetConfigSection<MessageBusSettings>();

        switch (settings.Provider)
        {
            case MessageBusProvider.RabbitMq:
                services.TryAddSingleton<IMessageBus, RabbitMqMessageBus>();
                services.AddHostedService<RabbitMqMessageBusConsumer>();
                break;
            case MessageBusProvider.Channel:
            default:
                services.TryAddSingleton<ChannelMessageBus>();
                services.TryAddSingleton<IMessageBus>(sp => sp.GetRequiredService<ChannelMessageBus>());
                services.AddHostedService<ChannelMessageBusDispatcher>();
                break;
        }

        return services;
    }
}