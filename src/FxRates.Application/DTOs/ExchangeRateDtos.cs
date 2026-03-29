namespace FxRates.Application.DTOs;

/// <summary>
/// What the API returns to the client
/// </summary>
public record ExchangeRateResponse(
    Guid Id,
    string FromCurrency,
    string ToCurrency,
    decimal BidPrice,
    decimal AskPrice,
    DateTime LastUpdated
);

/// <summary>
/// What the client sends to create an exchange rate
/// </summary>
public record CreateExchangeRateRequest(
    string FromCurrency,
    string ToCurrency,
    decimal BidPrice,
    decimal AskPrice
);

/// <summary>
/// What the client sends to update the prices
/// </summary>
public record UpdateExchangeRateRequest(
    decimal BidPrice,
    decimal AskPrice
);