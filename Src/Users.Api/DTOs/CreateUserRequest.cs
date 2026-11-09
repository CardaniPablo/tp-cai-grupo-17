namespace Users.Api.DTOs
{
    public record CreateUserRequest(
        string Nombre,
        string Apellido,
        string Email,
        string PasswordHash
    );
}