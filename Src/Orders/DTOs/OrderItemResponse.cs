namespace Orders.Api.DTOs
{
    public record OrderItemResponse(
    Guid ProductoId,
    int Cantidad,
    decimal PrecioUnitario
);
}
