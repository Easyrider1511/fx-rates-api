using FxRates.Domain.Entities;

namespace FxRates.Application.Services;

public interface IExchangeRateService
{
    /// <summary>Returns all stored exchange rates.</summary>
    Task<IEnumerable<ExchangeRate>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns the rate for the given currency pair.
    /// If not found in the database, fetches it from the external API and persists it.
    /// </summary>
    /// <param name="from">Source currency code (e.g. USD)</param>
    /// <param name="to">Target currency code (e.g. EUR)</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="KeyNotFoundException">Thrown when the pair is not found locally or externally.</exception>
    Task<ExchangeRate> GetOrFetchByPairAsync(string from, string to, CancellationToken ct = default);

    /// <summary>Manually creates a new exchange rate.</summary>
    /// <param name="from">Source currency code</param>
    /// <param name="to">Target currency code</param>
    /// <param name="bid">Bid price</param>
    /// <param name="ask">Ask price</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="InvalidOperationException">Thrown when the currency pair already exists.</exception>
    Task<ExchangeRate> CreateAsync(string from, string to, decimal bid, decimal ask, CancellationToken ct = default);

    /// <summary>Updates the Bid and Ask prices for an existing rate.</summary>
    /// <param name="id">Rate identifier</param>
    /// <param name="bid">New bid price</param>
    /// <param name="ask">New ask price</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="KeyNotFoundException">Thrown when no rate with the given ID exists.</exception>
    Task<ExchangeRate> UpdateAsync(Guid id, decimal bid, decimal ask, CancellationToken ct = default);

    /// <summary>Deletes the exchange rate with the given ID.</summary>
    /// <param name="id">Rate identifier</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="KeyNotFoundException">Thrown when no rate with the given ID exists.</exception>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}