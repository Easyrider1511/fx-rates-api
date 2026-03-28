using FxRates.Domain.Entities;

namespace FxRates.Domain.Repositories;

/// <summary>
/// Contract for the exchange rate repository.
/// The actual implementation is in the Infrastructure layer.
/// </summary>
public interface IExchangeRateRepository
{
    Task<ExchangeRate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ExchangeRate?> GetByCurrencyPairAsync(string from, string to, CancellationToken ct = default);
    Task<IEnumerable<ExchangeRate>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(ExchangeRate rate, CancellationToken ct = default);
    Task UpdateAsync(ExchangeRate rate, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}