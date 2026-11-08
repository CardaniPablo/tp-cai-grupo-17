using Notificaciones.Api.Extensions.HealthChecks;
using Notificaciones.Api.Services;

namespace Notificaciones.Api.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddAppServices(this IServiceCollection services)
        {
            services.AddSingleton<DatabaseInitializer>();
            
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddHealthChecks()
    .AddCheck<Notificaciones.Api.Extensions.HealthChecks.SqliteHealthCheck>(
        name: "SQLite-Notificaciones",
        tags: new[] { "ready" } 
    );


            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(600);
                setup.AddHealthCheckEndpoint("MiApi", "/health");
            }).AddInMemoryStorage();


        }
       


    }
}