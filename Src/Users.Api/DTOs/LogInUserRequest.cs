namespace Users.Api.DTOs
{
    public record LogInUserRequest(
       string Email,
       string PasswordHash
   );
}