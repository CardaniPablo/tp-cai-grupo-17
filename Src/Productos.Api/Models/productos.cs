using System.ComponentModel.DataAnnotations;

namespace Productos.Api.Models;

// ── Entidad principal ─────────────────────────
public record Producto
{
    public Guid Id { get; init; }  // Identificador único
    required public string Nombre { get; init; } = string.Empty;   // Requerido, máx. 100 caracteres
    public string? Descripcion { get; init; } // Opcional, máx. 500 caracteres
    public required decimal Precio { get; init; }   // Requerido, mayor a 0
    public int Stock { get; init; }  // Requerido, mayor o igual a 0
    public required string Categoria { get; init; } = string.Empty;   // Requerido
    public DateTime FechaCreacion{ get; init;}   // Asignado automáticamente al crear
}


