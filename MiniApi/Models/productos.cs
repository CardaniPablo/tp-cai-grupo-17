using System.ComponentModel.DataAnnotations;

namespace MiniApi.Models;

// ── Entidad principal ─────────────────────────
public record Producto
{
    public long Id  { get; init; }  // Identificador único
    public string Nombre { get; init; } = string.Empty;   // Requerido, máx. 100 caracteres
    public string? Descripcion { get; init; } // Opcional, máx. 500 caracteres
    public decimal Precio { get; init; }   // Requerido, mayor a 0
    public int Stock { get; init; }  // Requerido, mayor o igual a 0
    public string Categoria{ get; init; } = string.Empty;   // Requerido
    public DateTime FechaCreacion{ get; init;}   // Asignado automáticamente al crear
}


// ── Request para crear un producto (POST) ─────────────────────────────────────
public record CreateProductoRequest(
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock,
    string Categoria
);

// ── Request para actualizar un producto (PUT) ─────────────────────────────────
public record UpdateProductoRequest(
    string? Descripcion,
    decimal Precio,
    int Stock
);
