using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using static System.Net.WebRequestMethods;

namespace Notificaciones.Api.ExceptionHandlers;

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
        const string errorDefaultCode = "NTF-004";

        using (LogContext.PushProperty("errorCode", errorDefaultCode))
        using (LogContext.PushProperty("Endpoint", endpoint))
        {
            // Fallos inesperados del sistema se registran estrictamente como ERROR
            _logger.LogError(exception, "Error no controlado en Notificaciones.Api: {Message}", exception.Message);
        }
        // ----------------------------------

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://www.rfc-editor.org/info/rfc7231/#section-6.6.1",
            title = "Internal Server Error",
            status = StatusCodes.Status500InternalServerError,
            detail = "Ocurrió un error inesperado al procesar la notificación.",
            instance = context.Request.Path.Value,
            errorCode = errorDefaultCode,
            errorMessage = "Error interno al procesar la notificación."
        }, cancellationToken);

        return true;
    }
}