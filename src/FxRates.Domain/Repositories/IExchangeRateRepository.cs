using FxRates.Domain.Entities;

namespace FxRates.Domain.Repositories;

/// <summary>
/// Contract for the exchange rate repository.
/// The actual implementation is in the Infrastructure layer.
/// </summary>
public interface IExchangeRateRepository
{
    /// <summary>Returns the rate with the given ID, or <c>null</c> if not found.</summary>
    Task<ExchangeRate?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns the rate for the given currency pair, or <c>null</c> if not found.</summary>
    Task<ExchangeRate?> GetByCurrencyPairAsync(string from, string to, CancellationToken ct = default);

    /// <summary>Returns all stored exchange rates.</summary>
    Task<IEnumerable<ExchangeRate>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Persists a new exchange rate.</summary>
    Task AddAsync(ExchangeRate rate, CancellationToken ct = default);

    /// <summary>Persists changes to an existing exchange rate.</summary>
    Task UpdateAsync(ExchangeRate rate, CancellationToken ct = default);

    /// <summary>Removes the exchange rate with the given ID.</summary>
    /// <exception cref="KeyNotFoundException">Thrown when no rate with the given ID exists.</exception>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}