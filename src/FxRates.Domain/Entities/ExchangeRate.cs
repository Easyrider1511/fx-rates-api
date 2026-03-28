namespace FxRates.Domain.Entities;

/// <summary>
/// Represents an exchange rate between two currency pairs.
/// Bid = buying price | Ask = selling price.
/// </summary>
public class ExchangeRate
{
    public Guid Id { get; private set; }
    public string FromCurrency { get; private set; } = string.Empty;
    public string ToCurrency { get; private set; } = string.Empty;
    public decimal BidPrice { get; private set; }
    public decimal AskPrice { get; private set; }
    public DateTime LastUpdated { get; private set; } 
    
    private ExchangeRate() { }
    
    /// <summary>
    /// Only point of entitie creation.
    /// </summary>
    public static ExchangeRate Create(
        string fromCurrency,
        string toCurrency,
        decimal bidPrice,
        decimal askPrice)
    {
        if (string.IsNullOrWhiteSpace(fromCurrency))
            throw new ArgumentException("The source currency is required.");
        if (string.IsNullOrWhiteSpace(toCurrency))
            throw new ArgumentException("The target currency is required.");
        if (bidPrice <= 0)
            throw new ArgumentException("The Bid price must be positive.");
        if (askPrice <= 0)
            throw new ArgumentException("The Ask price must be positive.");

        return new ExchangeRate
        {
            Id           = Guid.NewGuid(),
            FromCurrency = fromCurrency.ToUpperInvariant(),
            ToCurrency   = toCurrency.ToUpperInvariant(),
            BidPrice     = bidPrice,
            AskPrice     = askPrice,
            LastUpdated  = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Update prices while keeping the same Id.
    /// </summary>
    public void UpdatePrices(decimal bidPrice, decimal askPrice)
    {
        if (bidPrice <= 0) throw new ArgumentException("The Bid price must be positive.");
        if (askPrice <= 0) throw new ArgumentException("The Ask price must be positive.");
        BidPrice    = bidPrice;
        AskPrice    = askPrice;
        LastUpdated = DateTime.UtcNow;
    }
}