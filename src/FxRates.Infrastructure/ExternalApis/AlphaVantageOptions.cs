using System.Text.Json;
using FxRates.Application.ExternalApis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FxRates.Infrastructure.ExternalApis;

// Configuration class — credentials come from appsettings.json
public class AlphaVantageOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://www.alphavantage.co";
}

public class AlphaVantageClient : IForexApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AlphaVantageOptions _options;
    private readonly ILogger<AlphaVantageClient> _logger;

    public AlphaVantageClient(
        HttpClient httpClient,
        IOptions<AlphaVantageOptions> options,
        ILogger<AlphaVantageClient> logger)
    {
        _httpClient = httpClient;
        _options    = options.Value;
        _logger     = logger;
    }

    public async Task<ForexRateDto?> GetRateAsync(
        string fromCurrency,
        string toCurrency,
        CancellationToken ct = default)
    {
        var url = $"{_options.BaseUrl}/query" +
                  $"?function=CURRENCY_EXCHANGE_RATE" +
                  $"&from_currency={fromCurrency}" +
                  $"&to_currency={toCurrency}" +
                  $"&apikey={_options.ApiKey}";

        _logger.LogInformation("Querying AlphaVantage: {From}/{To}", fromCurrency, toCurrency);

        try
        {
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            // AlphaVantage returns the data inside this specific key
            if (!doc.RootElement.TryGetProperty("Realtime Currency Exchange Rate", out var rateElement))
            {
                _logger.LogWarning("AlphaVantage did not return data for {From}/{To}", fromCurrency, toCurrency);
                return null;
            }

            var bidStr = rateElement.GetProperty("8. Bid Price").GetString() ?? "0";
            var askStr = rateElement.GetProperty("9. Ask Price").GetString() ?? "0";

            return new ForexRateDto(
                FromCurrency: fromCurrency.ToUpperInvariant(),
                ToCurrency:   toCurrency.ToUpperInvariant(),
                BidPrice:     decimal.Parse(bidStr, System.Globalization.CultureInfo.InvariantCulture),
                AskPrice:     decimal.Parse(askStr, System.Globalization.CultureInfo.InvariantCulture)
            );
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while querying AlphaVantage for {From}/{To}", fromCurrency, toCurrency);
            throw;
        }
    }
}