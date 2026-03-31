namespace FxRates.Application.Messaging;

/// <summary>
/// Event raised every time a new exchange rate is persisted in the database,
/// whether created manually (POST) or fetched automatically from the external API.
/// </summary>
public record RateAddedEvent(
    Guid     Id,
    string   FromCurrency,
    string   ToCurrency,
    decimal  BidPrice,
    decimal  AskPrice,
    DateTime OccurredAt);
