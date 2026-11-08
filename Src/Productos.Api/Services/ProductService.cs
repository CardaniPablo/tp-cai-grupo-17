using Productos.Api.DTOs;
using Productos.Api.Exeptions;
using Productos.Api.Models;


namespace Productos.Api.Services;

public class ProductService
{
    private readonly ProductoRepository _repo;

    public ProductService(ProductoRepository repo) => _repo = repo;

    public async Task<IEnumerable<Producto>> GetAllProductsAsync()
    {
        return await _repo.GetAllAsync();
    }

    public async Task<ProductosResponseDto> GetProductByIdAsync(Guid id)
    {
        var producto = await _repo.GetByIdAsync(id);

        if (producto is null)
            throw new NotFoundException("PRD-001", "Producto no encontrado.");

        return new ProductosResponseDto(
            Guid.Parse(producto.Id.ToString()),
            producto.Nombre,
            producto.Descripcion ?? string.Empty,
            producto.Precio,
            producto.Stock,
            producto.Categoria
        );
    }

    public async Task<ProductosResponseDto> CreateProductAsync(CreateProductoRequest req)
    {
        
        if (string.IsNullOrWhiteSpace(req.Nombre) || req.Precio <= 0 || req.Stock < 0 || string.IsNullOrWhiteSpace(req.Categoria))
            throw new BusinessRuleException("PRD-002", "Los datos del producto son inválidos.");

        if (req.Nombre.Length > 100 || (req.Descripcion is not null && req.Descripcion.Length > 500))
            throw new BusinessRuleException("PRD-002", "Los datos del producto son inválidos. Largo de caracteres excedido.");

        
        var productosExistentes = await _repo.GetAllAsync();
        var esDuplicado = productosExistentes.Any(p =>
            p.Nombre.Equals(req.Nombre, StringComparison.OrdinalIgnoreCase) &&
            p.Categoria.Equals(req.Categoria, StringComparison.OrdinalIgnoreCase));

        if (esDuplicado)
            throw new BusinessRuleException("PRD-003", "Ya existe un producto con ese nombre en la categoría.");

        var producto = await _repo.CreateAsync(req);

        return new ProductosResponseDto(
            Guid.Parse(producto.Id.ToString()),
            producto.Nombre,
            producto.Descripcion ?? string.Empty,
            producto.Precio,
            producto.Stock,
            producto.Categoria
        );
    }

    public async Task UpdateProductAsync(Guid id, UpdateProductoRequest req)
    {
      
        if (req.Precio <= 0 || req.Stock < 0)
            throw new BusinessRuleException("PRD-002", "Los datos del producto son inválidos.");

        var updated = await _repo.UpdateAsync(id, req);

        if (!updated)
            throw new NotFoundException("PRD-001", "Producto no encontrado.");
    }

    public async Task DeleteProductAsync(Guid id)
    {
 

        var deleted = await _repo.DeleteAsync(id);

        if (!deleted)
            throw new NotFoundException("PRD-001", "Producto no encontrado.");
    }
}