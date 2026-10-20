namespace MiniApi.Models;

// ── Entidad principal ─────────────────────────
public record User
{
    public Guid Id { get; init; }  // Identificador único
     public string Nombre { get; init; } = string.Empty;  // Requerido
    public string Apellido { get; init; } = string.Empty; // Requerido
    public string Email { get; init; } = string.Empty; // Requerido, único, formato válido
    public string PasswordHash { get; init; } = string.Empty; // Hash de la contraseña (nunca se expone en responses)
    public DateTime FechaRegistro { get; init; } // Asignado automáticamente al registrar
    public bool Activo { get; init; } // false cuando el usuario está bloqueado
    public int IntentosFallidos { get; init; }  // Se incrementa en cada login fallido; se resetea al loguearse OK


}


// ── Request para crear un User (POST) ─────────────────────────────────────
public record CreateUserRequest(
    string Nombre,
    string Apellido,
    string EMail,
    string PasswordHash
);

// ── Request para actualizar un User (Post) ─────────────────────────────────
public record LogInUserRequest(
    string mail,
    string PaswordHash
);
