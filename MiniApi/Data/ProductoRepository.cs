using Dapper;
using Microsoft.Data.Sqlite;
using MiniApi.Models;

namespace MiniApi.Data
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
                       created_at AS FechaCreacion
                FROM Productos
                ORDER BY created_at DESC
            """);
        }

        // ── GET BY ID ─────────────────────────────────────────────────────────
        public async Task<Producto?> GetByIdAsync(int Id)
        {
            using var conn = CreateConnection();
            // Corregido: seleccionamos 'id' en minúscula tal como existe en la DB y mapeamos con alias
            return await conn.QuerySingleOrDefaultAsync<Producto>("""
                SELECT Id, Nombre, Descripcion, Precio, Stock, Categoria,
                       created_at AS FechaCreacion
                FROM Productos
                WHERE Id = @Id
            """, new {Id});
        }

        // ── CREATE ────────────────────────────────────────────────────────────
        public async Task<Producto> CreateAsync(CreateProductoRequest request)
        {
            using var conn = CreateConnection();

            var id = await conn.ExecuteScalarAsync<int>("""
                INSERT INTO Productos ( Nombre, Descripcion, Precio, Stock, Categoria)
                VALUES ( @Nombre, @Descripcion, @Precio, @Stock, @Categoria);
                SELECT last_insert_rowid();
            """,request);

            return (await GetByIdAsync(id))!;
        }

        // ── UPDATE ────────────────────────────────────────────────────────────
        public async Task<bool> UpdateAsync(int Id, UpdateProductoRequest request)
        {
            using var conn = CreateConnection();

            var rows = await conn.ExecuteAsync("""
                UPDATE Productos
                SET
                    Descripcion = @Descripcion,
                    Precio       = @Precio,
                    Stock       = @Stock,
                    updated_at  = datetime('now')
                WHERE Id = @Id
            """, new { request.Descripcion, request.Precio, request.Stock, Id });

            return rows > 0;
        }

        // ── DELETE ────────────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(int Id)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync("DELETE FROM Productos WHERE Id = @Id", new { Id });
            return rows > 0;
        }
    }
}