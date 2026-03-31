using FluentAssertions;
using FxRates.Application.ExternalApis;
using FxRates.Application.Services;
using FxRates.Domain.Entities;
using FxRates.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FxRates.Tests.Services;

/// <summary>
/// Unit tests for ExchangeRateService.
/// Each test method verifies a specific scenario.
/// Naming convention: TestedMethod_Scenario_ExpectedResult
/// </summary>
public class ExchangeRateServiceTests
{
    // Mocks = fake versions of dependencies that we control in the tests
    private readonly Mock<IExchangeRateRepository> _repoMock = new();
    private readonly Mock<IForexApiClient>          _apiMock  = new();

    // SUT = System Under Test = what we are testing
    private readonly ExchangeRateService _sut;

    public ExchangeRateServiceTests()
    {
        // Create the service with the fake dependencies.
        _sut = new ExchangeRateService(
            _repoMock.Object,
            _apiMock.Object,
            NullLogger<ExchangeRateService>.Instance  // Logger that does nothing
        );
    }

    // ─── Unit tests: GetOrFetchByPairAsync ────────────────────────────────────────

    [Fact]
    public async Task GetOrFetchByPairAsync_WhenRateExistsInDb_ShouldReturnWithoutCallingApi()
    {
        // Arrange: set up the scenario
        var existingExchangeRate = ExchangeRate.Create("USD", "EUR", 0.91m, 0.92m);
        _repoMock
            .Setup(r => r.GetByCurrencyPairAsync("USD", "EUR", default))
            .ReturnsAsync(existingExchangeRate);

        // Act: execute the method we are testing
        var exchangeRateResult = await _sut.GetOrFetchByPairAsync("USD", "EUR");

        // Assert: verify the result
        exchangeRateResult.Should().Be(existingExchangeRate);  // should be the same object
        _apiMock.Verify(                        // the external API should NOT have been called
            a => a.GetRateAsync(It.IsAny<string>(), It.IsAny<string>(), default),
            Times.Never);
    }

    [Fact]
    public async Task GetOrFetchByPairAsync_WhenRateDoesNotExist_ShouldFetchFromApiAndStore()
    {
        // Arrange: repository does not have the exchange rate
        _repoMock
            .Setup(r => r.GetByCurrencyPairAsync("USD", "GBP", default))
            .ReturnsAsync((ExchangeRate?)null);

        // External API returns data
        _apiMock
            .Setup(a => a.GetRateAsync("USD", "GBP", default))
            .ReturnsAsync(new ForexRateDto("USD", "GBP", 0.78m, 0.79m));

        // Act
        var exchangeRateResult = await _sut.GetOrFetchByPairAsync("USD", "GBP");

        // Assert
        exchangeRateResult.BidPrice.Should().Be(0.78m);
        exchangeRateResult.AskPrice.Should().Be(0.79m);
        exchangeRateResult.FromCurrency.Should().Be("USD");

        // Verify that it was saved in the database
        _repoMock.Verify(
            r => r.AddAsync(It.IsAny<ExchangeRate>(), default),
            Times.Once);
    }

    [Fact]
    public async Task GetOrFetchByPairAsync_WhenApiDoesNotFindPair_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetByCurrencyPairAsync("XYZ", "ABC", default))
            .ReturnsAsync((ExchangeRate?)null);

        _apiMock
            .Setup(a => a.GetRateAsync("XYZ", "ABC", default))
            .ReturnsAsync((ForexRateDto?)null);  // API not found

        // Act & Assert: expect an exception to be thrown
        await _sut
            .Invoking(s => s.GetOrFetchByPairAsync("XYZ", "ABC"))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }

    // ─── Unit tests: CreateAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WhenCurrencyPairExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var existingExchangeRate = ExchangeRate.Create("EUR", "JPY", 160m, 161m);
        _repoMock
            .Setup(r => r.GetByCurrencyPairAsync("EUR", "JPY", default))
            .ReturnsAsync(existingExchangeRate);

        // Act & Assert
        await _sut
            .Invoking(s => s.CreateAsync("EUR", "JPY", 160m, 161m))
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");  // the message must contain “already exists”
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateAndStore()
    {
        // Arrange: repository does not have the currency pair
        _repoMock
            .Setup(r => r.GetByCurrencyPairAsync("BTC", "USD", default))
            .ReturnsAsync((ExchangeRate?)null);

        // Act
        var exchangeRateResult = await _sut.CreateAsync("BTC", "USD", 42000m, 42100m);

        // Assert
        exchangeRateResult.Should().NotBeNull();
        exchangeRateResult.FromCurrency.Should().Be("BTC");
        exchangeRateResult.BidPrice.Should().Be(42000m);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<ExchangeRate>(), default), Times.Once);
    }

    // ─── Unit tests: UpdateAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_WhenIdNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange: ID does not exist in the repository
        _repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((ExchangeRate?)null);

        // Act & Assert
        await _sut
            .Invoking(s => s.UpdateAsync(Guid.NewGuid(), 1m, 1m))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdatePrices()
    {
        // Arrange
        var exchangeRate = ExchangeRate.Create("USD", "EUR", 0.91m, 0.92m);
        _repoMock
            .Setup(r => r.GetByIdAsync(exchangeRate.Id, default))
            .ReturnsAsync(exchangeRate);

        // Act
        var result = await _sut.UpdateAsync(exchangeRate.Id, 0.95m, 0.96m);
        // Assert
        result.BidPrice.Should().Be(0.95m);
        result.AskPrice.Should().Be(0.96m);
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<ExchangeRate>(), default), Times.Once);
    }

    // ─── Unit tests: DeleteAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_WhenIdNotFound_ThrowsKeyNotFoundException()
    {
        _repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((ExchangeRate?)null);

        await _sut
            .Invoking(s => s.DeleteAsync(Guid.NewGuid()))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }
}