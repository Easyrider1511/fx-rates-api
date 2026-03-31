using FxRates.Application.Messaging;
using MassTransit;

namespace FxRates.Infrastructure.Messaging;

/// <summary>Implements IEventPublisher by delegating to the MassTransit publish endpoint.</summary>
public class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
        => publishEndpoint.Publish(message, ct);
}
