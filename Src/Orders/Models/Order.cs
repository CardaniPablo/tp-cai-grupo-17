using Orders.Api.Models;

namespace Orders.Api.Models;

public record Order { 
    public required Guid Id { get; init; }
    public required Guid UsuarioId { get; init; }
    public required OrderItem[] Items { get; init; }
    public required decimal Total { get; init; }
    public required string Estado { get; init; }
    public required DateTime FechaCreacion { get; init; }



}


