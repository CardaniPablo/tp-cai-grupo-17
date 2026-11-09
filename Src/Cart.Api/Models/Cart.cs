namespace Cart.Api.Models;

public record Cart
{
    public Guid IdUsuario { get; set; }

    public required CartItem[] Items { get; set; }

    public DateTime FechaActualizacion { get; set; }
}