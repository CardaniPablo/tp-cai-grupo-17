namespace Productos.Api.DTOs
{
    public record ProductosResponseDto(
        Guid IdProducto,
        string Nombre,
        string Descripcion,
        decimal Precio,
        int Stock,
        string Categoria
    );
}
