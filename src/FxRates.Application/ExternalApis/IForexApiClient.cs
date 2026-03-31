namespace FxRates.Application.ExternalApis;

public interface IForexApiClient
{
    /// <summary>
    /// Fetches the current exchange rate for a currency pair from the external API.
    /// Returns <c>null</c> if the pair is not found.
    /// </summary>
    /// <param name="fromCurrency">Source currency code (e.g. USD)</param>
    /// <param name="toCurrency">Target currency code (e.g. EUR)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A <see cref="ForexRateDto"/> with the current rates, or <c>null</c> if the pair was not found.</returns>
    Task<ForexRateDto?> GetRateAsync(
        string fromCurrency,
        string toCurrency,
        CancellationToken ct = default);
}

/// <summary>
/// Record used to transport data from the external API
/// </summary>
public record ForexRateDto(
    string FromCurrency,
    string ToCurrency,
    decimal BidPrice,
    decimal AskPrice
);