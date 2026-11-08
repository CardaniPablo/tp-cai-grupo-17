using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Productos.Api.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var endpoint = context.GetEndpoint()?.DisplayName ?? $"{context.Request.Method} {context.Request.Path}";
        const string errorDefaultCode = "PRD-005"; 

        using (LogContext.PushProperty("errorCode", errorDefaultCode))
        using (LogContext.PushProperty("Endpoint", endpoint))
        {
            _logger.LogError(exception, "Error no controlado catastrófico en Products.Api: {Message}", exception.Message);
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://www.rfc-editor.org/info/rfc7231/#section-6.6.1",
            title = "Internal Server Error",
            status = StatusCodes.Status500InternalServerError,
            detail = "Ocurrió un error inesperado en el servidor al procesar el catálogo de productos.",
            instance = context.Request.Path.Value,
            errorCode = errorDefaultCode,
            errorMessage = "Error interno del servidor en el módulo de Productos."
        }, cancellationToken);

        return true;
    }
}