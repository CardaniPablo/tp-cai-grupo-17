namespace Cart.Api.DTOs
{
    public record AñadirItemRequest(
            Guid IdProducto,
            int Cantidad
        );

}
