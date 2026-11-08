
using Orders.Api.Data;
using Orders.Api.DTOs;
using Orders.Api.Exceptions;
using Orders.Api.Models;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace Orders.Api.Services;

public class OrdersService
{
    private readonly OrdersRepository _repository;
    private readonly IHttpClientFactory _httpFactory;

    public OrdersService(OrdersRepository repository, IHttpClientFactory httpFactory)
    {
        _repository = repository;
        _httpFactory = httpFactory;
    }

    public async Task<IEnumerable<OrderResponse>> GetOrdersByUserIdAsync(Guid usuarioId)
        => (await _repository.GetByUserIdAsync(usuarioId)).Select(MapToResponse);

    public async Task<OrderResponse?> GetOrderByIdAsync(Guid id)
        => await _repository.GetByIdAsync(id) is { } orden ? MapToResponse(orden) : null;

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        var nuevaOrden = await ValidarYConstruirOrdenAsync(request);
        await _repository.CreateAsync(nuevaOrden);
        return MapToResponse(nuevaOrden);
    }

    public async Task<ActualizarEstadoResponse?> UpdateOrderStatusAsync(Guid id, string nuevoEstado)
        => await ValidarTransicionEstadoAsync(id, nuevoEstado)
            ? new ActualizarEstadoResponse(id, nuevoEstado, DateTime.UtcNow)
            : null;

    private async Task<Orders.Api.Models.Order> ValidarYConstruirOrdenAsync(CreateOrderRequest request)
    {
        // AJUSTE ACÁ: Cambiado a BusinessRuleException para acoplarse a ORD-003 según la tabla
        if (!await ExisteUsuarioRemoto(request.UsuarioId))
            throw new BusinessRuleException("ORD-003", "Usuario no encontrado al crear la orden.");

        var itemsValidados = new List<OrderItem>();
        decimal total = 0;

        foreach (var item in request.Items)
        {
            // AJUSTE ACÁ: Cambiado a BusinessRuleException para acoplarse a ORD-004 según la tabla
            var prod = await ObtenerProductoRemoto(item.ProductoId)
                ?? throw new BusinessRuleException("ORD-004", "Producto no encontrado al crear la orden.");

            if (item.Cantidad > prod.Stock)
                throw new BusinessRuleException("ORD-005", "Stock insuficiente para uno o más productos.");

            itemsValidados.Add(new OrderItem { ProductoId = item.ProductoId, Cantidad = item.Cantidad, PrecioUnitario = prod.Precio });
            total += item.Cantidad * prod.Precio;
        }

        return new Orders.Api.Models.Order
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId,
            Items = itemsValidados.ToArray(),
            Total = total,
            Estado = "Pendiente",
            FechaCreacion = DateTime.UtcNow
        };
    }

    private async Task<bool> ValidarTransicionEstadoAsync(Guid id, string nuevoEstado)
    {
        var orden = await _repository.GetByIdAsync(id) ?? throw new NotFoundException("ORD-001", "Orden no encontrada.");

        if (orden.Estado is "Entregada" or "Cancelada")
            throw new BusinessRuleException("ORD-006", "El estado de la orden no puede ser modificado.");

        return await _repository.UpdateStatusAsync(id, nuevoEstado);
    }

    private static OrderResponse MapToResponse(Orders.Api.Models.Order o)
        => new(o.Id, o.UsuarioId, o.Items.Select(i => new OrderItemResponse(i.ProductoId, i.Cantidad, i.PrecioUnitario)).ToList(), o.Total, o.Estado, o.FechaCreacion);

    private async Task<bool> ExisteUsuarioRemoto(Guid id)
    {
        var client = _httpFactory.CreateClient();
        var res = await client.GetAsync($"https://localhost:7067/User/{id}");

        if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }

        res.EnsureSuccessStatusCode();
        return true;
    }

    private async Task<(string Nombre, int Stock, decimal Precio)?> ObtenerProductoRemoto(Guid id)
    {
        var client = _httpFactory.CreateClient();
        var res = await client.GetAsync($"https://localhost:7048/Producto/{id}");

        if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadFromJsonAsync<JsonNode>();
        return (json?["nombre"]?.ToString() ?? "", json?["stock"]?.GetValue<int>() ?? 0, json?["precio"]?.GetValue<decimal>() ?? 0);
    }
}