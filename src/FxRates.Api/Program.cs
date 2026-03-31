using FxRates.Api.Middleware;
using FxRates.Application.ExternalApis;
using FxRates.Application.Services;
using FxRates.Domain.Repositories;
using FxRates.Infrastructure.ExternalApis;
using FxRates.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── SQLite database ─────────────────────────────────────────────────────────
// Reads the connection string from appsettings.json
builder.Services.AddDbContext<FxRatesDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("Default")
        ?? "Data Source=fxrates.db"));

// ─── Repositories ─────────────────────────────────────────────────────────────
// Scoped = one instance per HTTP request
builder.Services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();

// ─── Application services ───────────────────────────────────────────────────
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();

// ─── HTTP Client (AlphaVantage) ─────────────────────────────────────────────
// Configure<T> reads the "AlphaVantage" section from appsettings.json
builder.Services.Configure<AlphaVantageOptions>(
    builder.Configuration.GetSection("AlphaVantage"));

// AddHttpClient manages the HTTP connection pool (avoids issues with multiple requests)
builder.Services.AddHttpClient<IForexApiClient, AlphaVantageClient>();

// ─── Global error handling ──────────────────────────────────────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ─── Swagger ─────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "FxRates API",
        Version     = "v1",
        Description = "API for managing exchange rates (Forex). " +
                      "Fetches data from AlphaVantage and persists locally."
    });
    // Include XML comments from the controllers in Swagger
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

// ═════════════════════════════════════════════════════════════════════════════
var app = builder.Build();

// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FxRatesDbContext>();
    db.Database.Migrate();
}

// Show Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FxRates API v1"));
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();