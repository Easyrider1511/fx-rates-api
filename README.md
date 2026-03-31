# FxRates API

A RESTful API built with ASP.NET Core for managing foreign exchange rates (Forex).  
Supports full CRUD operations on currency pairs, with automatic fallback to [AlphaVantage](https://www.alphavantage.co) when a rate is not available locally.

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [API Endpoints](#api-endpoints)
- [Running the Tests](#running-the-tests)
- [Limitations & Possible Improvements](#limitations--possible-improvements)

---

## Features

- ✅ Full CRUD for exchange rates — each currency pair stores a **Bid** and **Ask** price
- ✅ Automatic fetch from AlphaVantage when a rate is not found locally
- ✅ Persistent local cache — once fetched, rates are stored and reused
- ✅ SQLite database via Entity Framework Core
- ✅ Global error handling with RFC 7807 ProblemDetails responses
- ✅ Interactive API documentation via Swagger / OpenAPI
- ✅ Unit tests with xUnit, Moq and FluentAssertions

---

## Architecture

The project follows **Clean Architecture**, separating concerns across four layers:

```
FxRates.Domain          ← Entities, repository contracts (no external dependencies)
FxRates.Application     ← Business logic, use cases, service interfaces
FxRates.Infrastructure  ← EF Core, HttpClient, AlphaVantage integration
FxRates.Api             ← Controllers, middleware, DI configuration
```

### Request flow for `GET /api/exchangerates/{from}/{to}`

```
HTTP Request
    → Controller          (receives and routes the request)
    → Service             (applies business logic)
    → Repository          (checks the local database)
        → [if not found]  → AlphaVantage API
        → [store result]  → Database
    → DTO                 (maps entity to response shape)
HTTP Response
```

The Service is the only layer that decides whether to go to the external API. The Controller does not contain business logic — it only delegates to the Service and returns the result.

---

## Tech Stack

| Technology | Version | Purpose |
|---|---|---|
| ASP.NET Core | 10.0 | Web framework |
| Entity Framework Core | 10.x | ORM / database access |
| SQLite | — | Local database |
| Swashbuckle | 10.x | Swagger / OpenAPI docs |
| xUnit | 2.x | Unit test framework |
| Moq | 4.x | Mocking in tests |
| FluentAssertions | 8.x | Readable test assertions |

---

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version matching the project's target framework)
- A free API key from [AlphaVantage](https://www.alphavantage.co/support/#api-key)

---

## Getting Started

```bash
# 1. Clone the repository
git clone https://github.com/Easyrider1511/fx-rates-api.git
cd fx-rates-api

# 2. Configure your AlphaVantage API key (see Configuration below)

# 3. Run the API
cd src/FxRates.Api
dotnet run
```

> Database migrations are applied automatically on startup — no manual migration step required.

The API will be available at:
- **HTTP**: `http://localhost:5254`
- **HTTPS**: `https://localhost:7077`
- **Swagger UI**: `http://localhost:5254/swagger` *(Development environment only)*

---

## Configuration

### AlphaVantage API Key

**Never commit your API key to a public repository.**  
Use .NET User Secrets to store it locally:

```bash
cd src/FxRates.Api
dotnet user-secrets init
dotnet user-secrets set "AlphaVantage:ApiKey" "YOUR_API_KEY_HERE"
```

The `appsettings.json` file contains a placeholder that is safe to commit:

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=fxrates.db"
  },
  "AlphaVantage": {
    "ApiKey": "SET_VIA_USER_SECRETS",
    "BaseUrl": "https://www.alphavantage.co"
  }
}
```

---

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/exchangerates` | List all stored rates |
| `GET` | `/api/exchangerates/{from}/{to}` | Get rate by pair (fetches from AlphaVantage if not found locally) |
| `POST` | `/api/exchangerates` | Manually create a rate |
| `PUT` | `/api/exchangerates/{id}` | Update Bid/Ask prices for an existing rate |
| `DELETE` | `/api/exchangerates/{id}` | Delete a rate by ID |

### Example — Create a rate

```http
POST /api/exchangerates
Content-Type: application/json

{
  "fromCurrency": "USD",
  "toCurrency": "EUR",
  "bidPrice": 0.9154,
  "askPrice": 0.9162
}
```

**Response `201 Created`:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fromCurrency": "USD",
  "toCurrency": "EUR",
  "bidPrice": 0.9154,
  "askPrice": 0.9162,
  "lastUpdated": "2026-03-31T10:00:00Z"
}
```

### Example — Update prices

```http
PUT /api/exchangerates/3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json

{
  "bidPrice": 0.9200,
  "askPrice": 0.9210
}
```

**Response `200 OK`:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fromCurrency": "USD",
  "toCurrency": "EUR",
  "bidPrice": 0.9200,
  "askPrice": 0.9210,
  "lastUpdated": "2026-03-31T11:00:00Z"
}
```

### Example — Auto-fetch from AlphaVantage

```http
GET /api/exchangerates/USD/EUR
```

If `USD/EUR` does not exist in the database, the API will automatically fetch it from AlphaVantage, store it, and return it. Subsequent calls return the cached result from the database directly.

### Error responses

All errors follow the [RFC 7807 ProblemDetails](https://datatracker.ietf.org/doc/html/rfc7807) format:

```json
{
  "status": 404,
  "title": "Resource not found",
  "detail": "Currency pair USD/XYZ was not found."
}
```

---

## Running the Tests

```bash
dotnet test --verbosity normal
```

### Test coverage

The unit tests cover the core scenarios of `ExchangeRateService`:

| Scenario | Expected behaviour |
|---|---|
| Rate exists in database | Returns immediately, never calls external API |
| Rate not in database | Fetches from AlphaVantage, persists, and returns |
| External API returns null | Throws `KeyNotFoundException` → 404 |
| Creating a duplicate pair | Throws `InvalidOperationException` → 409 |
| Updating a non-existent ID | Throws `KeyNotFoundException` → 404 |
| Deleting a non-existent ID | Throws `KeyNotFoundException` → 404 |

---

## Limitations & Possible Improvements

| Limitation | Suggested improvement |
|---|---|
| AlphaVantage free tier allows 25 requests/day | Add TTL-based caching with `IMemoryCache` or Redis |
| No authentication | Add JWT Bearer token authentication |
| SQLite is not suitable for high-load production | Switch EF Core provider to PostgreSQL or SQL Server |
| No pagination on `GET /exchangerates` | Add `pageNumber` / `pageSize` query parameters |
| No formal input validation | Integrate FluentValidation |
| Rates can become stale over time | Add a `BackgroundService` to periodically refresh stored pairs |
| No rate history | Add an audit table to track price changes over time |
