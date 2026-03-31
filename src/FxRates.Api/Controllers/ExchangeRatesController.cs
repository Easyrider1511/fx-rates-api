using FxRates.Application.DTOs;
using FxRates.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FxRates.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExchangeRatesController : ControllerBase
{
    private readonly IExchangeRateService _service;
    private readonly ILogger<ExchangeRatesController> _logger;

    public ExchangeRatesController(
        IExchangeRateService service,
        ILogger<ExchangeRatesController> logger)
    {
        _service = service;
        _logger  = logger;
    }

    /// <summary>Returns all exchange rates stored in the database.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ExchangeRateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var rates = await _service.GetAllAsync(ct);
        return Ok(rates.Select(MapToResponse));
    }

    /// <summary>
    /// Gets the exchange rate for a specific currency pair.
    /// If it does not exist in the database, it fetches it from the external API and stores it automatically.
    /// </summary>
    /// <param name="from">Source currency (e.g., USD)</param>
    /// <param name="to">Target currency (e.g., EUR)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("{from}/{to}")]
    [ProducesResponseType(typeof(ExchangeRateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByPair(string from, string to, CancellationToken ct)
    {
        var rate = await _service.GetOrFetchByPairAsync(from, to, ct);
        return Ok(MapToResponse(rate));
    }

    /// <summary>Creates a new exchange rate manually.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ExchangeRateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateExchangeRateRequest request, CancellationToken ct)
    {
        var rate = await _service.CreateAsync(
            request.FromCurrency, request.ToCurrency,
            request.BidPrice, request.AskPrice, ct);

        // 201 Created with the Location header pointing to the new resource
        return CreatedAtAction(
            nameof(GetByPair),
            new { from = rate.FromCurrency, to = rate.ToCurrency },
            MapToResponse(rate));
    }

    /// <summary>Updates the prices of an existing exchange rate.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ExchangeRateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateExchangeRateRequest request, CancellationToken ct)
    {
        var rate = await _service.UpdateAsync(id, request.BidPrice, request.AskPrice, ct);
        return Ok(MapToResponse(rate));
    }

    /// <summary>Deletes an exchange rate by ID.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();  // 204 = success with no response body
    }

    /// <summary>Maps a domain entity to the API response DTO.</summary>
    private static ExchangeRateResponse MapToResponse(
        FxRates.Domain.Entities.ExchangeRate r)
        => new(r.Id, r.FromCurrency, r.ToCurrency, r.BidPrice, r.AskPrice, r.LastUpdated);
}