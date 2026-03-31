using FxRates.Application.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace FxRates.Infrastructure.Messaging;

public static class MessagingExtensions
{
    /// <summary>
    /// Registers MassTransit with the in-memory transport and the RateAddedConsumer.
    /// To switch to RabbitMQ, replace UsingInMemory with UsingRabbitMq and configure the host.
    /// </summary>
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<RateAddedConsumer>();

            x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
        });

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        return services;
    }
}
