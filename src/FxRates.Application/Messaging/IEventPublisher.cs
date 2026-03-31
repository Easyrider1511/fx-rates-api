namespace FxRates.Application.Messaging;

/// <summary>Publishes domain events to the message bus.</summary>
public interface IEventPublisher
{
    /// <summary>Publishes a message of type <typeparamref name="T"/> to the bus.</summary>
    /// <typeparam name="T">The message type</typeparam>
    /// <param name="message">The message to publish</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task that completes when the message has been dispatched to the bus.</returns>
    Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class;
}
