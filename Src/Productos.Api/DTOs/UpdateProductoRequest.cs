namespace Productos.Api.DTOs
{

    // ── Request para actualizar un producto (PUT) ─────────────────────────────────
    public record UpdateProductoRequest(
        string? Descripcion,
        decimal Precio,
        int Stock
    );
}
