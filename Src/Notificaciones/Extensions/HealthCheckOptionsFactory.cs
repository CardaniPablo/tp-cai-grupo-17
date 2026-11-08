using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

namespace Notificaciones.Api.Extensions;

public static class HealthCheckOptionsFactory
{
    public static HealthCheckOptions CreateJsonOptions() => new()
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(), // Healthy | Degraded | Unhealthy
                duration = report.TotalDuration.ToString(),
                entries = report.Entries.Select(e => new
                {
                    key = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.ToString()
                })
            }, new JsonSerializerOptions { WriteIndented = true });

            await context.Response.WriteAsync(json);
        }
    };
}