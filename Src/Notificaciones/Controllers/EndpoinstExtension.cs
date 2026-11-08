using Notificaciones.Api.Endpoints;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Notificaciones.Api.Extensions;

namespace Notificaciones.Api.Controllers;

public static class EndpointsExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        // ── ENDPOINTS DE NEGOCIO ─────────────────────────────────────────
        app.MapNotificationEndpoints();

        // ── ENDPOINTS DE HEALTH CHECKS (FORZADOS COMO GET) ────────────────

        // 1. /health/live -> Liveness Probe
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = HealthCheckOptionsFactory.CreateJsonOptions().ResponseWriter
        });

        // 2. /health/ready -> Readiness Probe
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("database"),
            ResponseWriter = HealthCheckOptionsFactory.CreateJsonOptions().ResponseWriter
        });
    }
}
