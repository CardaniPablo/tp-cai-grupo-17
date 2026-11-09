namespace Cart.Api.DTOs
{
    public record ActualizarItemRequest
    (
        Guid UsuarioId,
        Guid IdProducto,
        int Cantidad
    );
}
