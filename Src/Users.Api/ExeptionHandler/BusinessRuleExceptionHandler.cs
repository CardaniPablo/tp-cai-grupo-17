using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Threading;
using System.Threading.Tasks;
using Users.Api.Exceptions;

namespace Users.Api.ExceptionHandlers;

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

        // ── 1. ASIGNACIÓN DINÁMICA DE CÓDIGOS, TÍTULOS Y URLS DE TIPO (TP OFICIAL) ──
        var (statusCode, title, typeUrl) = ex.ErrorCode switch
        {
            "USR-001" => (StatusCodes.Status409Conflict, "Conflict", "https://tools.ietf.org/html/rfc7231#section-6.5.9"),
            "USR-002" => (StatusCodes.Status400BadRequest, "Bad Request", "https://www.rfc-editor.org/info/rfc7231/#section-6.5.1"),
            "USR-003" => (StatusCodes.Status401Unauthorized, "Unauthorized", "https://tools.ietf.org/html/rfc7235#section-3.1"),
            "USR-004" => (StatusCodes.Status403Forbidden, "Forbidden", "https://tools.ietf.org/html/rfc7231#section-6.5.3"),
            "USR-005" => (StatusCodes.Status403Forbidden, "Forbidden", "https://tools.ietf.org/html/rfc7231#section-6.5.3"),
            _ => (StatusCodes.Status400BadRequest, "Bad Request", "https://www.rfc-editor.org/info/rfc7231/#section-6.5.1")
        };

        context.Response.StatusCode = statusCode;

        var endpoint = context.GetEndpoint()?.DisplayName ?? $"{context.Request.Method} {context.Request.Path}";

        using (LogContext.PushProperty("errorCode", ex.ErrorCode))
        using (LogContext.PushProperty("Endpoint", endpoint))
        {
            _logger.LogWarning(exception, "Regla de negocio violada en Usuarios: {Message}", ex.Message);
        }

        await context.Response.WriteAsJsonAsync(new
        {
            type = typeUrl, // Inyecta dinámicamente la URL específica requerida
            title = title,
            status = statusCode,
            detail = "No se pudo procesar la solicitud del usuario debido a una regla de negocio.",
            instance = context.Request.Path.Value,
            errorCode = ex.ErrorCode,
            errorMessage = ex.Message
        }, cancellationToken);

        return true;
    }
}