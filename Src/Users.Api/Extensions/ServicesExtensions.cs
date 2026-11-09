using Dapper;
using Users.Api.Extensions.HealthChecks;
using Users.Api.Services;

namespace Users.Api.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddAppServices(this IServiceCollection services)
        {
            
            services.AddSingleton<DatabaseInitializer>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            

            services.AddHealthChecks()
                .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database"])
                .AddCheck<ApiStatusCheck>("api-status", tags: ["api"]);

            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(600);
                setup.AddHealthCheckEndpoint("MiApi", "/health");
            }).AddInMemoryStorage();
        }
    }
}