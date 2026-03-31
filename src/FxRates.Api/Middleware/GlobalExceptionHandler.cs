using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FxRates.Api.Middleware;

/// <summary>
/// Catches all unhandled exceptions in the controllers.
/// Converts each exception type into the appropriate HTTP status code.
/// Prevents stack traces from being exposed to the exterior.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        _logger.LogError(exception, "Exception not handled: {Message}", exception.Message);

        // Map exception type to HTTP status code
        var (status, title) = exception switch
        {
            KeyNotFoundException      => (StatusCodes.Status404NotFound,      "Resource not found"),
            ArgumentException         => (StatusCodes.Status400BadRequest,     "Invalid request"),
            InvalidOperationException => (StatusCodes.Status409Conflict,       "Data conflict"),
            HttpRequestException      => (StatusCodes.Status502BadGateway,     "Error contacting external API"),
            _                         => (StatusCodes.Status500InternalServerError, "Internal server error")
        };

        httpContext.Response.StatusCode = status;

        // ProblemDetails is the standard format (RFC 7807) for API errors.
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title  = title,
            Detail = exception.Message
        }, ct);

        return true; // true = "Already handled, do not propagate it."
    }
}