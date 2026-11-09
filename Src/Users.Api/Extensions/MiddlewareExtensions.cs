using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

namespace Users.Api.Extensions
{
    public static class MiddlewareExtensions
    {
        public static void UseAppMiddleware(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

          
            app.UseMiddleware<CorrelationIdMiddleware>();

            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (httpContext, _, ex) =>
                   
                    (ex != null || httpContext.Response.StatusCode >= 500)
                        ? Serilog.Events.LogEventLevel.Error :
                        (httpContext.Request.Path.StartsWithSegments("/health"))
                            ? Serilog.Events.LogEventLevel.Verbose : Serilog.Events.LogEventLevel.Information;
            });

            app.UseMiddleware<AuditMiddleware>();

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.MapHealthChecksUI(setup => setup.UIPath = "/health-ui");
        }
    }
}
