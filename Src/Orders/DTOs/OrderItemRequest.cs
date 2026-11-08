namespace Orders.Api.DTOs
{
    public record OrderItemRequest(
    Guid ProductoId,
    int Cantidad
);
}
