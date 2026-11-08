using Microsoft.AspNetCore.Mvc;
using Orders.Api.Data;
using Orders.Api.DTOs;
using Orders.Api.Models;
using Orders.Api.Services;
using Orders.Api.Exceptions;

namespace Orders.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrdersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders");

        // ── LISTAR ÓRDENES (?usuarioId=) ───────────────────────────────────
        group.MapGet("/", async ([FromQuery] Guid? usuarioId, OrdersService service) =>
        {
            if (usuarioId == null || usuarioId == Guid.Empty)
                throw new BusinessRuleException("ORD-002", "El parámetro usuarioId es requerido para filtrar.");

            var response = await service.GetOrdersByUserIdAsync(usuarioId.Value);
            return Results.Ok(response);
        });

        // ── OBTENER DETALLE DE UNA ORDEN ───────────────────────────────────
        group.MapGet("/{id:guid}", async (Guid id, OrdersService service) =>
        {
            var response = await service.GetOrderByIdAsync(id);
            if (response is null)
                throw new NotFoundException("ORD-001", "Orden no encontrada.");

            return Results.Ok(response);
        });

        // ── CREAR NUEVA ORDEN (CHECKOUT) ───────────────────────────────────
        group.MapPost("/", async ([FromBody] CreateOrderRequest request, OrdersService service) =>
        {
            if (request == null || request.UsuarioId == Guid.Empty || request.Items == null || !request.Items.Any())
                throw new BusinessRuleException("ORD-002", "Los datos de la orden son inválidos o la lista de ítems está vacía.");

            // Toda la validación síncrona y la llamada a EnsureSuccessStatusCode ocurre de forma segura en el Service
            var response = await service.CreateOrderAsync(request);
            return Results.Json(response, statusCode: 201);
        });

        // ── ACTUALIZAR ESTADO DE LA ORDEN ────────────────────────────────────
        group.MapPut("/{id:guid}/status", async (Guid id, [FromBody] ActualizarEstadoRequest request, OrdersService service) =>
        {
            var response = await service.UpdateOrderStatusAsync(id, request.Estado);
            if (response is null)
                throw new BusinessRuleException("ORD-007", "Error interno al actualizar el estado o transición no válida.");

            return Results.Ok(response);
        });
    }
}