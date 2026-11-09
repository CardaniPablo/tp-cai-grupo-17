using Cart.Api.Data;
using Cart.Api.DTOs;
using Cart.Api.Models;
using Cart.Api.Exceptions;
using System.Net.Http.Json;
namespace Cart.Api.Services;

public class CartService
{
    private readonly CartRepository _repository;
    private readonly IHttpClientFactory _httpFactory;

    public CartService(CartRepository repository, IHttpClientFactory httpFactory)
    {
        _repository = repository;
        _httpFactory = httpFactory;
    }

    public Task<Cart.Api.Models.Cart?> GetByUserIdAsync(VerTuCarritoRequest request)
        => _repository.GetByUserIdAsync(request.UsuarioId);

    

    public async Task AddItemAsync(Guid usuarioId, Guid idProducto, int cantidad)
    {
   
        if (!await ExisteUsuarioRemotoAsync(usuarioId))
            throw new NotFoundException("CRT-001", $"El usuario con ID {usuarioId} no existe en el sistema.");

        var stockDisponible = await ObtenerStockRemotoAsync(idProducto);
        if (stockDisponible == null)
            throw new NotFoundException("CRT-004", $"El producto con ID {idProducto} no existe en el catálogo.");

        if (cantidad > stockDisponible.Value)
            throw new BusinessRuleException("CRT-003", $"No se puede añadir al carrito. Cantidad solicitada: {cantidad}, Stock disponible: {stockDisponible.Value}.");

        var carritoExistente = await _repository.GetByUserIdAsync(usuarioId);

        if (carritoExistente is null)
        {
            await _repository.CreateAsync(usuarioId);
        }


        await _repository.AddItemAsync(usuarioId, idProducto, cantidad);
    }

    // ── MODIFICAR CANTIDAD
    public async Task<bool> UpdateItemAsync(ActualizarItemRequest request)
    {
        // 1. Consultamos el stock actual del producto en el catálogo remoto
        var stockDisponible = await ObtenerStockRemotoAsync(request.IdProducto);
        if (stockDisponible == null)
            throw new NotFoundException("CRT-004", $"El producto con ID {request.IdProducto} no existe en el catálogo.");

        // 2. REQUERIMIENTO LÓGICO: Validar que la nueva cantidad no rompa el stock de la estantería
        if (request.Cantidad > stockDisponible.Value)
            throw new BusinessRuleException("CRT-003", $"No se puede actualizar la cantidad. Nueva cantidad solicitada: {request.Cantidad}, Stock disponible: {stockDisponible.Value}.");

        // 3. Si pasa el control, persistimos el cambio en SQLite de forma segura
        return await _repository.UpdateItemAsync(
            request.UsuarioId,
            request.IdProducto,
            request.Cantidad);
    }

    public Task<bool> RemoveItemAsync(
        EliminarItemRequest request)
        => _repository.RemoveItemAsync(
            request.UsuarioId,
            request.IdProducto);

    public Task<bool> ClearCartAsync(EliminarCarritoRequest request)
        => _repository.ClearCartAsync(request.UsuarioId);


    private async Task<bool> ExisteUsuarioRemotoAsync(Guid id)
    {
        var client = _httpFactory.CreateClient();
        var res = await client.GetAsync($"https://localhost:7067/User/{id}");

        if (res.StatusCode == System.Net.HttpStatusCode.NotFound) return false;

        res.EnsureSuccessStatusCode();
        return true;
    }

    private async Task<int?> ObtenerStockRemotoAsync(Guid id)
    {
        var client = _httpFactory.CreateClient();
        var res = await client.GetAsync($"https://localhost:7048/Producto/{id}");

        if (res.StatusCode == System.Net.HttpStatusCode.NotFound) return null;

        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadFromJsonAsync<System.Text.Json.Nodes.JsonNode>();
        return json?["stock"]?.GetValue<int>();
    }
}