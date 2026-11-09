namespace Cart.Api.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }
}
