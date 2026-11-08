namespace Notificaciones.Api.DTOs
{
    public record NotificacionesResponse(
     Guid Id,
     Guid UsuarioId,
     string Mensaje,
     string Tipo,
     string Estado,
     DateTime FechaEnvio
 );
}
