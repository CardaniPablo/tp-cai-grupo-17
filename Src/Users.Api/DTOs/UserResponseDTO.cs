namespace Users.Api.DTOs
{
    public record UserResponseDto(
    Guid Id,
    string Nombre,
    string Apellido,
    string Email,
    DateTime FechaRegistro
    );
}
