using Dapper;
using Microsoft.Data.Sqlite;
using Notificaciones.Api.Models;

namespace Notificaciones.Api.Data;

public class NotificationsRepository
{
    private readonly IConfiguration _config;

    public NotificationsRepository(IConfiguration config) => _config = config;

    private SqliteConnection CreateConnection() =>
        new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");

    public async Task CreateAsync(Notification notification)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            INSERT INTO Notificaciones (Id, UsuarioId, Mensaje, Tipo, Estado, FechaEnvio)
            VALUES (@Id, @UsuarioId, @Mensaje, @Tipo, @Estado, @FechaEnvio)
        """, new
        {
            Id = notification.Id.ToString(),
            UsuarioId = notification.UsuarioId.ToString(),
            notification.Mensaje,
            notification.Tipo,
            notification.Estado,
            FechaEnvio = notification.FechaEnvio.ToString("o")
        });
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
    {
        using var conn = CreateConnection();
        var rows = await conn.QueryAsync("""
            SELECT Id, UsuarioId, Mensaje, Tipo, Estado, FechaEnvio
            FROM Notificaciones
            WHERE LOWER(UsuarioId) = LOWER(@UserId)
            ORDER BY FechaEnvio DESC
        """, new { UserId = userId.ToString() });

        return rows.Select(r => new Notification
        {
            Id = Guid.Parse(r.Id.ToString()),
            UsuarioId = Guid.Parse(r.UsuarioId.ToString()),
            Mensaje = r.Mensaje.ToString(),
            Tipo = r.Tipo.ToString(),
            Estado = r.Estado.ToString(),
            FechaEnvio = DateTime.Parse(r.FechaEnvio.ToString())
        });
    }
}