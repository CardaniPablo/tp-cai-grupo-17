using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;
using Productos.Api.DTOs;
using Productos.Api.Models;

namespace Productos.Api.Services
{
    public class ProductoRepository
    {
        private readonly IConfiguration _config;

        public ProductoRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");

        // ── GET ALL ───────────────────────────────────────────────────────────
        public async Task<IEnumerable<Producto>> GetAllAsync()
        {
            using var conn = CreateConnection();
            // Forzamos el alias exacto 'id' coincidiendo con tu record en minúscula
            return await conn.QueryAsync<Producto>("""
                SELECT Id, Nombre, Descripcion, Precio, Stock, Categoria,
                       FechaCreacion AS FechaCreacion
                FROM Productos
                ORDER BY FechaCreacion DESC
            """);
        }

        // ── GET BY ID ─────────────────────────────────────────────────────────
        public async Task<Producto?> GetByIdAsync(Guid Id)
        {
            using var conn = CreateConnection();
            // Corregido: seleccionamos 'id' en minúscula tal como existe en la DB y mapeamos con alias
            return await conn.QuerySingleOrDefaultAsync<Producto>("""
                SELECT Id, Nombre, Descripcion, Precio, Stock, Categoria,
                       FechaCreacion AS FechaCreacion
                FROM Productos
                WHERE Id = @Id
            """, new {Id=Id.ToString()});
        }
        
        // ── CREATE ────────────────────────────────────────────────────────────
        public async Task<Producto> CreateAsync(CreateProductoRequest request)
        {
            using var conn = CreateConnection();
            Guid nuevoId = Guid.NewGuid();

            await conn.ExecuteAsync("""
                INSERT INTO Productos (Id, Nombre, Descripcion, Precio, Stock, Categoria)
                VALUES (@Id, @Nombre, @Descripcion, @Precio, @Stock, @Categoria);
            """, new
            {
                Id = nuevoId.ToString(),
                request.Nombre,
                request.Descripcion,
                request.Precio,
                request.Stock,
                request.Categoria
            });

            return (await GetByIdAsync(nuevoId))!;
        }

        // ── UPDATE ────────────────────────────────────────────────────────────
        public async Task<bool> UpdateAsync(Guid Id, UpdateProductoRequest request)
        {
            using var conn = CreateConnection();

            var rows = await conn.ExecuteAsync("""
                UPDATE Productos
                SET
                    Descripcion = @Descripcion,
                    Precio       = @Precio,
                    Stock       = @Stock
                WHERE Id = @Id
            """, new { request.Descripcion, request.Precio, request.Stock, Id = Id.ToString() });

            return rows > 0;
        }

        // ── DELETE ────────────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(Guid Id)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync("DELETE FROM Productos WHERE Id = @Id", new { Id = Id.ToString() });
            return rows > 0;
        }

    }
}