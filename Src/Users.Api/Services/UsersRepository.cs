using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;
using Users.Api.DTOs;
using Users.Api.Models;

namespace Users.Api.Data
{
    public class UserRepository
    {
        private readonly IConfiguration _config;

        public UserRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");

        // ── Get All ──────────────────────────────────────────────────────────
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<User>("""
                SELECT Id, Nombre, Apellido, Email, 
                       (Activo = 1) AS Activo, 
                       IntentosFallidos, FechaRegistro
                FROM Users
                ORDER BY FechaRegistro DESC
            """);
        }

        // ── Log In ─────────────────────────────────────────────────────────
        public async Task<User?> LogInAsync(LogInUserRequest request)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<User>("""
                SELECT Id, Nombre, Apellido, Email, PasswordHash, 
                       (Activo = 1) AS Activo, 
                       IntentosFallidos, FechaRegistro
                FROM Users
                WHERE Email = @Email AND PasswordHash = @PasswordHash
            """, new { request.Email, request.PasswordHash });
        }

        // ── CREATE ────────────────────────────────────────────────────────────
        public async Task<User> CreateAsync(CreateUserRequest request)
        {
            using var conn = CreateConnection();
            Guid nuevoId = Guid.NewGuid();
            await conn.ExecuteAsync("""
                INSERT INTO Users (Id, Nombre, Apellido, Email, PasswordHash, Activo, IntentosFallidos)
                VALUES (@Id, @Nombre, @Apellido, @Email, @PasswordHash, 1, 0);
            """, new
            {
                Id = nuevoId.ToString(),
                request.Nombre,
                request.Apellido,
                request.Email,
                request.PasswordHash
            });

            return (await GetByIdAsync(nuevoId))!;
        }

        // ── GET BY ID ─────────────────────────────────────────────────────────
        public async Task<User?> GetByIdAsync(Guid Id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<User>("""
                SELECT Id, Nombre, Apellido, Email, PasswordHash, 
                       (Activo = 1) AS Activo, 
                       IntentosFallidos, FechaRegistro
                FROM Users
                WHERE Id = @Id
            """, new { Id = Id.ToString() });
        }

        public async Task<User?> GetByEmailAsync(string Email)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<User>("""
                SELECT Id, Nombre, Apellido, Email, PasswordHash, Activo, IntentosFallidos, FechaRegistro
                FROM Users
                WHERE Email = @Email
            """, new { Email });
        }

        // ── ACTUALIZAR ESTADO DE BLOQUEO ──────────────────────────────────────
        public async Task UpdateBlockStatusAsync(Guid Id, int intentosFallidos, bool activo)
        {
            using var conn = CreateConnection();

            var queryParameters = new DynamicParameters();
            queryParameters.Add("intentosFallidos", intentosFallidos, DbType.Int32);
            queryParameters.Add("activo", activo ? 1 : 0, DbType.Int32);
            queryParameters.Add("Id", Id.ToString(), DbType.String);

            await conn.ExecuteAsync("""
                UPDATE Users
                SET IntentosFallidos = @intentosFallidos,
                    Activo = @activo
                WHERE Id = @Id
            """, queryParameters);
        }
    }
}