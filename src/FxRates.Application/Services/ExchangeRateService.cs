using FxRates.Application.ExternalApis;
using FxRates.Domain.Entities;
using FxRates.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FxRates.Application.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly IExchangeRateRepository _repository;
    private readonly IForexApiClient _forexApiClient;
    private readonly ILogger<ExchangeRateService> _logger;
    
    // Constructor dependency injection
    public ExchangeRateService(
        IExchangeRateRepository repository,
        IForexApiClient forexApiClient,
        ILogger<ExchangeRateService> logger)
    {
        _repository     = repository;
        _forexApiClient = forexApiClient;
        _logger         = logger;
    }

    public Task<IEnumerable<ExchangeRate>> GetAllAsync(CancellationToken ct = default)
        => _repository.GetAllAsync(ct);

    /// <summary>
    /// Core challenge logic:
    /// 1. Checks whether the exchange rate already exists in the database
    /// 2. If it exists, returns it directly (local cache)
    /// 3. If it does not exist, fetches it from the external API and stores it in the database
    /// </summary>
    public async Task<ExchangeRate> GetOrFetchByPairAsync(
        string from, string to, CancellationToken ct = default)
    {
        var existing = await _repository.GetByCurrencyPairAsync(from, to, ct);
        if (existing is not null)
        {
            _logger.LogInformation("Rate {From}/{To} found in the database.", from, to);
            return existing;
        }

        _logger.LogInformation("Rate {From}/{To} not found. Querying external API…", from, to);
        var dto = await _forexApiClient.GetRateAsync(from, to, ct);

        if (dto is null)
            throw new KeyNotFoundException($"Currency pair {from}/{to} was not found.");

        var rate = ExchangeRate.Create(dto.FromCurrency, dto.ToCurrency, dto.BidPrice, dto.AskPrice);
        await _repository.AddAsync(rate, ct);

        _logger.LogInformation("Rate {From}/{To} saved with ID {Id}.", from, to, rate.Id);
        return rate;
    }

    public async Task<ExchangeRate> CreateAsync(
        string from, string to, decimal bid, decimal ask, CancellationToken ct = default)
    {
        var existing = await _repository.GetByCurrencyPairAsync(from, to, ct);
        if (existing is not null)
            throw new InvalidOperationException($"The {from}/{to} pair already exists. Use the update endpoint.");

        var rate = ExchangeRate.Create(from, to, bid, ask);
        await _repository.AddAsync(rate, ct);
        return rate;
    }

    public async Task<ExchangeRate> UpdateAsync(
        Guid id, decimal bid, decimal ask, CancellationToken ct = default)
    {
        var rate = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Rate with ID {id} not found.");

        rate.UpdatePrices(bid, ask);
        await _repository.UpdateAsync(rate, ct);
        return rate;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var rate = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Rate with ID {id} not found.");

        await _repository.DeleteAsync(rate.Id, ct);
    }
}