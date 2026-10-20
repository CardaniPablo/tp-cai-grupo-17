using Dapper;
using Microsoft.Data.Sqlite;
using MiniApi.Models;

namespace MiniApi.Data
{
    public class UserRepository
    {
        private readonly IConfiguration _config;

        public UserRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");


        // ── GET BY ID ─────────────────────────────────────────────────────────
        public async Task<User?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<User>("""
            SELECT id, Nombre, Apellido, Email,
                   created_at AS CreatedAt
            FROM Users
            WHERE id = @id
        """, new { id });
        }
        // ── CREATE ────────────────────────────────────────────────────────────
        public async Task<User> CreateAsync(CreateUserRequest request)
        {
            using var conn = CreateConnection();

            var id = await conn.ExecuteScalarAsync<int>("""
            INSERT INTO items (Nombre, Apellido, Email, PasswordHash)
            VALUES (@Nombre, @Apellido, @Email, @PasswordHash);
            SELECT last_insert_rowid();
        """, request);

            return (await GetByIdAsync(id))!;
        }

       

    }
}