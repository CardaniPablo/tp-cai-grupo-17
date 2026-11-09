namespace Cart.Api.Models;

public record CartItem
{
    public required Guid IdProducto { get; init; }

    public required int Cantidad { get; set; }
}


