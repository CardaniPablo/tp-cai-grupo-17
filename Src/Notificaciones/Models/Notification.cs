namespace Notificaciones.Api.Models
{
    public class Notification
    {
        public Guid Id { get; set; }   // Identificador único
        public Guid UsuarioId { get; set; }   // Usuario destinatario
        public string Mensaje { get; set; }   // Requerido, máx. 500 caracteres
        public string Tipo { get; set; }   // Email | Push | SMS
        public string Estado { get; set; }   // Pendiente | Enviada | Fallida
        public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;  // Asignado automáticamente al registrar
    }
}


