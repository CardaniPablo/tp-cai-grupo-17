using Microsoft.AspNetCore.Diagnostics;
using Notificaciones.Api.Exceptions;
using Serilog.Context;
using Microsoft.Extensions.Logging;

namespace Notificaciones.Api.ExceptionHandlers;

public class BusinessRuleExceptionHandler : IExceptionHandler
{
    private readonly ILogger<BusinessRuleExceptionHandler> _logger;

    public BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleException ex) return false;

        // ── MAPEO DE CÓDIGOS HTTP Y URLS SEGÚN LA TABLA OFICIAL DE NOTIFICACIONES ──
        var (statusCode, title, typeUrl) = ex.ErrorCode switch
        {
            "NTF-001" => (StatusCodes.Status404NotFound, "Not Found", "https://tools.ietf.org/html/rfc7231#section-6.5.4"),
            "NTF-002" => (StatusCodes.Status400BadRequest, "Bad Request", "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
            _ => (StatusCodes.Status400BadRequest, "Bad Request", "https://tools.ietf.org/html/rfc7231#section-6.5.1")
        };

        context.Response.StatusCode = statusCode;

        // --- REQUISITOS NO FUNCIONALES ---
        var endpoint = context.GetEndpoint()?.DisplayName ?? $"{context.Request.Method} {context.Request.Path}";

        using (LogContext.PushProperty("errorCode", ex.ErrorCode))
        using (LogContext.PushProperty("Endpoint", endpoint))
        {
            _logger.LogWarning(exception, "Regla de negocio violada: {Message}", ex.Message);
        }
        // ----------------------------------

        await context.Response.WriteAsJsonAsync(new
        {
            type = typeUrl,
            title = title,
            status = statusCode,
            detail = "No se pudo procesar la solicitud debido a una regla de negocio del catálogo de notificaciones.",
            instance = context.Request.Path.Value,
            errorCode = ex.ErrorCode,
            errorMessage = ex.Message
        }, cancellationToken);

        return true;
    }
}