using FxRates.Domain.Entities;

namespace FxRates.Infrastructure.Persistence;

public class FxRatesDbContext : DbContext
{
    public FxRatesDbContext(DbContextOptions<FxRatesDbContext> options) : base(options) { }

    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExchangeRate>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Unique index: two records with the same currency pair cannot exist
            entity.HasIndex(e => new { e.FromCurrency, e.ToCurrency })
                .IsUnique()
                .HasDatabaseName("IX_ExchangeRates_CurrencyPair");

            // Precision for exchange rate values (e.g. 0.921543)
            entity.Property(e => e.BidPrice).HasPrecision(18, 6);
            entity.Property(e => e.AskPrice).HasPrecision(18, 6);

            entity.Property(e => e.FromCurrency).HasMaxLength(10).IsRequired();
            entity.Property(e => e.ToCurrency).HasMaxLength(10).IsRequired();
        });
    }
}