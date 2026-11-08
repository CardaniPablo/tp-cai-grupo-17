namespace Orders.Api.DTOs
{
    public record CreateOrderRequest(
    Guid UsuarioId,
    List<OrderItemRequest> Items
);
}
