using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Productos.Api.Exeptions;
using Serilog.Context;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Productos.Api.ExceptionHandlers;

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

        // ── MAPEO DE CÓDIGOS HTTP Y URLS SEGÚN LA TABLA OFICIAL DE PRODUCTOS ──
        var (statusCode, title, typeUrl) = ex.ErrorCode switch
        {
            "PRD-002" => (StatusCodes.Status400BadRequest, "Bad Request", "https://www.rfc-editor.org/info/rfc7231/#section-6.5.1"),
            "PRD-003" => (StatusCodes.Status409Conflict, "Conflict", "https://tools.ietf.org/html/rfc7231#section-6.5.9"),
            "PRD-004" => (StatusCodes.Status409Conflict, "Conflict", "https://tools.ietf.org/html/rfc7231#section-6.5.9"),
            _ => (StatusCodes.Status400BadRequest, "Bad Request", "https://www.rfc-editor.org/info/rfc7231/#section-6.5.1")
        };

        context.Response.StatusCode = statusCode;

        var endpoint = context.GetEndpoint()?.DisplayName ?? $"{context.Request.Method} {context.Request.Path}";

        using (LogContext.PushProperty("errorCode", ex.ErrorCode))
        using (LogContext.PushProperty("Endpoint", endpoint))
        {
            _logger.LogWarning(exception, "Regla de negocio violada en Productos: {Message}", ex.Message);
        }

        await context.Response.WriteAsJsonAsync(new
        {
            type = typeUrl,
            title = title,
            status = statusCode,
            detail = "No se pudo procesar la solicitud debido a una regla de negocio del catálogo.",
            instance = context.Request.Path.Value,
            errorCode = ex.ErrorCode,
            errorMessage = ex.Message
        }, cancellationToken);

        return true;
    }
}