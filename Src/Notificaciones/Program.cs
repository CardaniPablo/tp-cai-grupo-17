using Dapper;
using Notificaciones.Api.Controllers;
using Notificaciones.Api.Data;
using Notificaciones.Api.Extensions;
using Notificaciones.Api.Services;


public partial class Program
{

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Logging
        builder.AddAppLogging();

        // 2. Servicios (Swagger, HealthChecks, Dapper, etc.)
        builder.Services.AddAppServices();
        builder.Services.AddScoped<NotificationsRepository>();
        builder.Services.AddScoped<NotificationService>();
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        builder.Services.AddExceptionHandler<Notificaciones.Api.ExceptionHandlers.NotFoundExceptionHandler>();
        builder.Services.AddExceptionHandler<Notificaciones.Api.ExceptionHandlers.BusinessRuleExceptionHandler>();
        builder.Services.AddExceptionHandler<Notificaciones.Api.ExceptionHandlers.GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        var app = builder.Build();
        


        // 3. Inicializar DB
        using (var scope = app.Services.CreateScope())
            scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();
        // Log de ejemplo
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError("OK");

        // 4. Middleware (Swagger UI, Serilog, HealthChecks)
        app.UseAppMiddleware();
        app.UseExceptionHandler();

        // 5. Endpoints
        app.MapAppEndpoints();
        app.Run();
    }
}