namespace Cart.Api.DTOs
{
    public record EliminarItemRequest
    (
        Guid UsuarioId,
        Guid IdProducto
    );
}
