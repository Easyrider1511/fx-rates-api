using FxRates.Application.Messaging;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FxRates.Infrastructure.Messaging;

/// <summary>
/// Consumes RateAddedEvent messages from the in-memory queue.
/// Replace or extend this consumer to integrate with external systems
/// (e.g. send notifications, trigger downstream services).
/// </summary>
public class RateAddedConsumer(ILogger<RateAddedConsumer> logger) : IConsumer<RateAddedEvent>
{
    public Task Consume(ConsumeContext<RateAddedEvent> context)
    {
        var e = context.Message;
        logger.LogInformation(
            "[Queue] New rate added: {From}/{To} — Bid: {Bid}, Ask: {Ask} (ID: {Id})",
            e.FromCurrency, e.ToCurrency, e.BidPrice, e.AskPrice, e.Id);
        return Task.CompletedTask;
    }
}
