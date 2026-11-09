namespace Users.Api.Models;

// ── Entidad principal ─────────────────────────
public record User
{
    public Guid Id { get; set; }  // Identificador único
     public string Nombre { get; set; } = string.Empty;  // Requerido
    public string Apellido { get; set; } = string.Empty; // Requerido
    public string Email { get; set; } = string.Empty; // Requerido, único, formato válido
    public string PasswordHash { get; set; } = string.Empty; // Hash de la contraseña (nunca se expone en responses)
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow; // Asignado automáticamente al registrar
    public bool Activo { get; set; } = true; // false cuando el usuario está bloqueado
    public int IntentosFallidos { get; set; } = 0; // Se incrementa en cada login fallido; se resetea al loguearse OK


}



