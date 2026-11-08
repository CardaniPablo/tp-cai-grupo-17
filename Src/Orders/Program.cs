using Orders.Api.Data;
using Orders.Api.Extensions;
using Orders.Api.Services;
using Orders.Api.Controllers;
using Dapper;


public partial class Program
{

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Logging
        builder.AddAppLogging();

        // 2. Servicios (Swagger, HealthChecks, Dapper, etc.)
        builder.Services.AddAppServices();
        builder.Services.AddScoped<OrdersRepository>();
        builder.Services.AddScoped<OrdersService>();
        var productsApiUrl = builder.Configuration["ServicesUrls:ProductsApi"] ?? "http://localhost:5113";

        builder.Services.AddHttpClient<ProductClient>(client =>
        {
            client.BaseAddress = new Uri(productsApiUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        var usersApiUrl = builder.Configuration["ServicesUrls:UsersApi"] ?? "http://localhost:5112";

        builder.Services.AddHttpClient<UserClient>(client =>
        {
            client.BaseAddress = new Uri(usersApiUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        builder.Services.AddHttpClient<IProductClient, ProductClient>(
            client =>
            {
                client.BaseAddress =
                    new Uri("https://localhost:7002/");
            });
        Dapper.SqlMapper.AddTypeHandler(new Orders.Api.Data.OrdersRepository.SqliteGuidTypeHandler());
        builder.Services.AddExceptionHandler<Orders.Api.ExceptionHandlers.NotFoundExceptionHandler>();
        builder.Services.AddExceptionHandler<Orders.Api.ExceptionHandlers.BusinessRuleExceptionHandler>();
        builder.Services.AddExceptionHandler<Orders.Api.ExceptionHandlers.GlobalExceptionHandler>();
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