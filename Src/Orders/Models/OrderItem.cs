
namespace Orders.Api.Models
{
    public record OrderItem
    {
        public required Guid ProductoId { get; init; }
        public required int Cantidad { get; init; }
        public required decimal PrecioUnitario { get; init; }


    }
}
