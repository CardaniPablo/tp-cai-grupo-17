using Dapper;
using Microsoft.Data.Sqlite;
using Orders.Api.Models;

namespace Orders.Api.Services
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
        CREATE TABLE IF NOT EXISTS Orders (
        idOrden              TEXT PRIMARY KEY,
        usuarioId       TEXT NOT NULL,
        total           REAL NOT NULL DEFAULT 0,
        estado          TEXT NOT NULL DEFAULT 'Pendiente',
        fechaCreacion   TEXT NOT NULL DEFAULT (datetime('now'))
        );
        """);

            
            connection.Execute("""
        CREATE TABLE IF NOT EXISTS OrderItems (
        idOrden         TEXT NOT NULL,
        productoId      TEXT NOT NULL,
        cantidad        INTEGER NOT NULL DEFAULT 1,
        precioUnitario  REAL NOT NULL,
        PRIMARY KEY (idOrden, productoId),
        FOREIGN KEY (idOrden) REFERENCES Orders(idOrden) ON DELETE CASCADE
        );
        """);

            _logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
        }


    }
}