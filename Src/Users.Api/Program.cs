using Dapper;
using Users.Api.Controllers;
using Users.Api.Data;
using Users.Api.Extensions;
using Users.Api.Services;

public partial class Program
{

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Logging
        builder.AddAppLogging();

        // 2. Servicios (Swagger, HealthChecks, Dapper, etc.)
        builder.Services.AddAppServices();
        builder.Services.AddScoped<UserRepository>();
        builder.Services.AddScoped<UserService>();
        SqlMapper.AddTypeHandler(new GuidTypeHandler());


        builder.Services.AddExceptionHandler<Users.Api.ExceptionHandlers.NotFoundExceptionHandler>();
        builder.Services.AddExceptionHandler<Users.Api.ExceptionHandlers.BusinessRuleExceptionHandler>();
        builder.Services.AddExceptionHandler<Users.Api.ExceptionHandlers.GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        // -----------------------------------------------------------------------------

        var app = builder.Build();

        // 3. Inicializar DB
        using (var scope = app.Services.CreateScope())
            scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();
       
        

        // 4. Middleware (Swagger UI, Serilog, HealthChecks)
        app.UseAppMiddleware();
        app.UseExceptionHandler();
        // -----------------------------------------------------------------------------

        // 5. Endpoints
        app.MapAppEndpoints();
        app.Run();
    }
}