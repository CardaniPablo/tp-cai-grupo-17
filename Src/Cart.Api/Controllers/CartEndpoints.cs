using Cart.Api.DTOs;
using Cart.Api.Services;
using Cart.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Cart.Api.Endpoints;

public static class CartEndpoints
{
    public static void MapCartEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/cart");

        // ── OBTENER CARRITO ───────────────────────────────────────────────
        group.MapGet("/{usuarioId:guid}", async (Guid usuarioId, CartService service) =>
        {
            if (usuarioId == Guid.Empty)
                throw new BusinessRuleException("CRT-002", "El ID de usuario provisto no es válido.");

            var carrito = await service.GetByUserIdAsync(new VerTuCarritoRequest(usuarioId));
            if (carrito is null)
                throw new NotFoundException("CRT-001", "El carrito para el usuario solicitado no existe.");

            return Results.Ok(carrito);
        });



        // ── AGREGAR PRODUCTO (CORREGIDO: Pasa parámetros sueltos al servicio) ──
        group.MapPost("/{usuarioId:guid}/agregar-producto", async (Guid usuarioId, [FromBody] AñadirItemRequest request, CartService service) =>
        {
            if (usuarioId == Guid.Empty || request.IdProducto == Guid.Empty)
                throw new BusinessRuleException("CRT-002", "Los identificadores de usuario o producto no pueden estar vacíos.");

            if (request.Cantidad <= 0)
                throw new BusinessRuleException("CRT-003", "La cantidad de ítems a añadir debe ser mayor a cero.");

            // Llamada limpia al servicio con los componentes individuales
            await service.AddItemAsync(usuarioId, request.IdProducto, request.Cantidad);
            return Results.Ok();
        });

        // ── MODIFICAR CANTIDAD ─────────────────────────────────────────────
        group.MapPatch("/{usuarioId:guid}/productos/{Idproducto:guid}/cantidad", async (Guid usuarioId, Guid Idproducto, [FromBody] ActualizarItemRequest request, CartService service) =>
        {
            if (request.Cantidad <= 0)
                throw new BusinessRuleException("CRT-003", "La nueva cantidad debe ser mayor a cero.");

            var actualizado = await service.UpdateItemAsync(new ActualizarItemRequest(usuarioId, Idproducto, request.Cantidad));
            if (!actualizado)
                throw new NotFoundException("CRT-004", "No se pudo actualizar porque el ítem no existe en el carrito.");

            return Results.Ok();
        });

        // ── ELIMINAR PRODUCTO ──────────────────────────────────────────────
        group.MapDelete("/{usuarioId:guid}/productos/{Idproducto:guid}", async (Guid usuarioId, Guid Idproducto, CartService service) =>
        {
            var eliminado = await service.RemoveItemAsync(new EliminarItemRequest(usuarioId, Idproducto));
            if (!eliminado)
                throw new NotFoundException("CRT-004", "El ítem solicitado no se encuentra en el carrito.");

            return Results.Ok();
        });

        // ── VACIAR CARRITO ─────────────────────────────────────────────────
        group.MapPost("/{usuarioId:guid}/vaciar", async (Guid usuarioId, CartService service) =>
        {
            var vaciado = await service.ClearCartAsync(new EliminarCarritoRequest(usuarioId));
            if (!vaciado)
                throw new NotFoundException("CRT-001", "No se pudo vaciar porque el carrito no existe.");

            return Results.Ok();
        });
    }
}