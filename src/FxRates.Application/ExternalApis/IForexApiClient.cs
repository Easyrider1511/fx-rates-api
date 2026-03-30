namespace FxRates.Application.ExternalApis;

public interface IForexApiClient
{
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