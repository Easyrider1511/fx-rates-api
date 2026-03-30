using FxRates.Domain.Entities;

namespace FxRates.Application.Services;

public interface IExchangeRateService
{
    Task<IEnumerable<ExchangeRate>> GetAllAsync(CancellationToken ct = default);
    Task<ExchangeRate> GetOrFetchByPairAsync(string from, string to, CancellationToken ct = default);
    Task<ExchangeRate> CreateAsync(string from, string to, decimal bid, decimal ask, CancellationToken ct = default);
    Task<ExchangeRate> UpdateAsync(Guid id, decimal bid, decimal ask, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);    
}