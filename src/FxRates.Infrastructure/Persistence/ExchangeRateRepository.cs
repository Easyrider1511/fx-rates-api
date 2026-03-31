using FxRates.Domain.Entities;
using FxRates.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FxRates.Infrastructure.Persistence;

public class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly FxRatesDbContext _context;

    public ExchangeRateRepository(FxRatesDbContext context)
    {
        _context = context;
    }

    public async Task<ExchangeRate?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.ExchangeRates.FindAsync(new object[] { id }, ct);

    public async Task<ExchangeRate?> GetByCurrencyPairAsync(
        string from, string to, CancellationToken ct = default)
        => await _context.ExchangeRates
            .FirstOrDefaultAsync(r =>
                r.FromCurrency == from.ToUpperInvariant() &&
                r.ToCurrency == to.ToUpperInvariant(), ct);

    public async Task<IEnumerable<ExchangeRate>> GetAllAsync(CancellationToken ct = default)
        => await _context.ExchangeRates.ToListAsync(ct);

    public async Task AddAsync(ExchangeRate rate, CancellationToken ct = default)
    {
        await _context.ExchangeRates.AddAsync(rate, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ExchangeRate rate, CancellationToken ct = default)
    {
        _context.ExchangeRates.Update(rate);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var rate = await GetByIdAsync(id, ct);
        if (rate is not null)
        {
            _context.ExchangeRates.Remove(rate);
            await _context.SaveChangesAsync(ct);
        }
    }    
}