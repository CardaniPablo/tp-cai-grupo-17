using Microsoft.AspNetCore.Diagnostics;
using Serilog.Context;
using Microsoft.Extensions.Logging;

namespace Orders.Api.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // --- REQUISITOS NO FUNCIONALES ---
        var endpoint = context.GetEndpoint()?.DisplayName ?? $"{context.Request.Method} {context.Request.Path}";
        const string errorDefaultCode = "ORD-007";

        using (LogContext.PushProperty("errorCode", errorDefaultCode))
        using (LogContext.PushProperty("Endpoint", endpoint))
        {
            // Errores imprevistos del sistema se registran estrictamente como ERROR
            _logger.LogError(exception, "Ocurrió un error no controlado en Orders.API: {Message}", exception.Message);
        }
        // ----------------------------------

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://www.rfc-editor.org/info/rfc7231/#section-6.6.1",
            title = "Internal Server Error",
            status = StatusCodes.Status500InternalServerError,
            detail = "Ocurrió un error inesperado al procesar la orden.",
            instance = context.Request.Path.Value,
            errorCode = errorDefaultCode,
            errorMessage = "Error interno al procesar la orden."
        }, cancellationToken);

        return true;
    }
}