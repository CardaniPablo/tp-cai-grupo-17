using Dapper;
using Microsoft.Data.Sqlite;

namespace Users.Api.Services
{
    public class DatabaseInitializer
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IConfiguration config, ILogger<DatabaseInitializer> logger)
        {
            _config = config;
            _logger = logger;
        }

        public void Initialize()
        {
            var connectionString = _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=app.db";

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // ALUMNO: en caso de tener que agregar otra tabla es duplicar este comando
            connection.Execute("""
            CREATE TABLE IF NOT EXISTS Users(
                Id                TEXT PRIMARY KEY,
                Nombre            TEXT NOT NULL,
                Apellido          TEXT NOT NULL,
                Email             TEXT NOT NULL UNIQUE,
                PasswordHash      TEXT NOT NULL,
                FechaRegistro     TEXT NOT NULL DEFAULT (datetime('now')),
                Activo            INTEGER NOT NULL DEFAULT 1, -- <-- CAMBIADO DE BIT A INTEGER
                IntentosFallidos  INTEGER NOT NULL DEFAULT 0
            );
        """);





            _logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
        }
    }
}
