using Dapper;
using Microsoft.Data.Sqlite;
using Cart.Api.Models;

namespace Cart.Api.Services
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
        CREATE TABLE IF NOT EXISTS Cart (
            IdUsuario           TEXT PRIMARY KEY,
            FechaActualizacion  TEXT
        );
    """);

            
            connection.Execute("""
        CREATE TABLE IF NOT EXISTS CartItems (
            IdUsuario           TEXT,
            IdProducto          TEXT,
            Cantidad            INTEGER NOT NULL DEFAULT 0,
            PRIMARY KEY (IdUsuario, IdProducto),
            FOREIGN KEY (IdUsuario) REFERENCES Cart(IdUsuario) ON DELETE CASCADE
        );
        """);


            _logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
        }


    }
}