using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System;
using System.Threading.Tasks;

namespace Notificaciones.Api.Extensions
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Buscar si el cliente envió el encabezado, si no, generar un GUID nuevo
            if (!context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            // 2. Insertar el ID en el contexto de Serilog para que salga en los archivos JSON
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                // 3. Dejar que la petición continúe su camino hacia los controladores
                await _next(context);
            }
        }
    }
}