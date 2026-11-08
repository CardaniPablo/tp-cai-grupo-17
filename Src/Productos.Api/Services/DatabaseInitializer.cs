using Dapper;
using Microsoft.Data.Sqlite;

namespace Productos.Api.Services
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
            CREATE TABLE IF NOT EXISTS Productos (
                Id          TEXT PRIMARY KEY,
                Nombre        TEXT    NOT NULL,
                Descripcion TEXT,
                Precio       REAL    NOT NULL DEFAULT 0,
                Stock       INTEGER NOT NULL DEFAULT 0,
                Categoria   TEXT    NOT NULL,
                FechaCreacion   TEXT    NOT NULL DEFAULT (datetime('now'))
            );
        """);



            _logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
        }
    }
}
