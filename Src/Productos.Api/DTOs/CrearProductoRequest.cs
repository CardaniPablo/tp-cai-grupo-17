namespace Productos.Api.DTOs
{
    // ── Request para crear un producto (POST) ─────────────────────────────────────
    public record CreateProductoRequest(
        string Nombre,
        string? Descripcion,
        decimal Precio,
        int Stock,
        string Categoria
    );


}
