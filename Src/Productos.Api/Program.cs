using Dapper;
using Productos.Api.Controllers;
using Productos.Api.Extensions;
using Productos.Api.Services;
using Productos.Api.ExceptionHandlers;

public partial class Program
{

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Logging
        builder.AddAppLogging();

        // 2. Servicios (Swagger, HealthChecks, Dapper, etc.)
        builder.Services.AddAppServices();
        builder.Services.AddScoped<ProductoRepository>();
        builder.Services.AddScoped<ProductService>();
        SqlMapper.AddTypeHandler(new GuidTypeHandler());

        // --- AGREGADO: Registro de ExceptionHandlers para Requisitos No Funcionales ---
        builder.Services.AddExceptionHandler<Productos.Api.ExceptionHandlers.NotFoundExceptionHandler>();
        builder.Services.AddExceptionHandler<Productos.Api.ExceptionHandlers.BusinessRuleExceptionHandler>();
        builder.Services.AddExceptionHandler<Productos.Api.ExceptionHandlers.GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        // -----------------------------------------------------------------------------

        var app = builder.Build();

        // 3. Inicializar DB
        using (var scope = app.Services.CreateScope())
            scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();
        // Log de ejemplo
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError("OK");

        // 4. Middleware (Swagger UI, Serilog, HealthChecks)
        app.UseAppMiddleware();

        // --- AGREGADO: Activación del pipeline de excepciones debajo de los middlewares ---
        app.UseExceptionHandler();
        // -----------------------------------------------------------------------------

        // 5. Endpoints
        app.MapAppEndpoints();
        app.Run();
    }
}