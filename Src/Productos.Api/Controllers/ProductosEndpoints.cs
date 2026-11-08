using Microsoft.AspNetCore.Mvc;
using Productos.Api.DTOs;
using Productos.Api.Services;

namespace Productos.Api.Controllers
{
    public static class ProductosEndpoints
    {
        public static void MapProductosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/Producto");

            // GET all
            group.MapGet("/", async (ProductService service) =>
            {
                var productos = await service.GetAllProductsAsync();
                return Results.Ok(productos);
            })
            .WithTags("Producto");

            // GET by id
            group.MapGet("/{id:guid}", async (Guid id, ProductService service) =>
            {
                var response = await service.GetProductByIdAsync(id);
                return Results.Ok(response);
            })
            .WithTags("Producto");

            // POST (Devuelve 201 Created cumpliendo el estándar REST)
            group.MapPost("/", async (CreateProductoRequest req, ProductService service) =>
            {
                var response = await service.CreateProductAsync(req);
                return Results.Created($"/Producto/{response.IdProducto}", response);
            })
            .WithTags("Producto");

            // PUT
            group.MapPut("/{id:guid}", async (Guid id, UpdateProductoRequest req, ProductService service) =>
            {
                await service.UpdateProductAsync(id, req);
                return Results.Ok();
            })
            .WithTags("Producto");

            // DELETE
            group.MapDelete("/{id:guid}", async (Guid id, ProductService service) =>
            {
                await service.DeleteProductAsync(id);
                return Results.Ok();
            })
            .WithTags("Producto");
        }
    }
}