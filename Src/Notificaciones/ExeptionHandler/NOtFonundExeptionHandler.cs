using Microsoft.AspNetCore.Diagnostics;
using Notificaciones.Api.Exceptions;
using Serilog.Context; 
using Microsoft.Extensions.Logging; 

namespace Notificaciones.Api.ExceptionHandlers;

public class NotFoundExceptionHandler : IExceptionHandler
{
    private readonly ILogger<NotFoundExceptionHandler> _logger;

    public NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException ex) return false;

        context.Response.StatusCode = StatusCodes.Status404NotFound;

        // --- REQUISITOS NO FUNCIONALES ---
        var endpoint = context.GetEndpoint()?.DisplayName ?? $"{context.Request.Method} {context.Request.Path}";

        using (LogContext.PushProperty("errorCode", ex.ErrorCode))
        using (LogContext.PushProperty("Endpoint", endpoint))
        {
            // Error de negocio controlado: nivel Warning
            _logger.LogWarning(exception, "Recurso no encontrado: {Message}", ex.Message);
        }
        // ----------------------------------

        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            title = "Not Found",
            status = StatusCodes.Status404NotFound,
            detail = "El recurso solicitado no fue encontrado.",
            instance = context.Request.Path.Value,
            errorCode = ex.ErrorCode,
            errorMessage = ex.Message
        }, cancellationToken);

        return true;
    }
}