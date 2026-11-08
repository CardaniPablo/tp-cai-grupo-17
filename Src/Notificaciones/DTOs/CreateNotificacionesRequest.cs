namespace Notificaciones.Api.DTOs
{
    // ── Request para crear un producto (POST) ─────────────────────────────────────
    public record CreateNotificacionesRequest(
     Guid UsuarioId,
     string Mensaje,
     string Tipo
 );

}
