using Microsoft.AspNetCore.Mvc;
using MiniApi.Data;
using MiniApi.Models;

namespace MiniApi.Extensions.Endpoints

{
    // ALUMNOS: clase donde se definen los endpoints para cada entidad
    public static class ProductosEndpointsExtension
    {
        public static void MapProductosEndpoints(this WebApplication app)
        {
            // GET all
            app.MapGet("/Producto", async ( ProductoRepository repo) =>
            {
                var Producto = await repo.GetAllAsync();
                return Results.Ok(Producto);
            })
            .WithTags("Producto");

            // GET by id
            app.MapGet("/Producto/{id}", async (int id,  ProductoRepository repo) =>
            {
                var Producto = await repo.GetByIdAsync(id);
                return Producto is not null ? Results.Ok(Producto) : Results.NotFound();
            })
            .WithTags("Producto");

            // POST
            app.MapPost("/Producto", async (CreateProductoRequest req, ProductoRepository repo) =>
            {

                if (req.Nombre is not null && req.Nombre.Length > 100)
                {
                    return Results.BadRequest("El nombre no puede superar los 100 caracteres.");
                }
                if (req.Descripcion is not null && req.Descripcion.Length > 500)
                {
                    return Results.BadRequest("La descripción no puede superar los 500 caracteres.");
                }

                if (req.Precio <= 0)
                {
                    return Results.BadRequest("El precio debe ser un valor mayor a 0.");
                }

                if (req.Stock < 0)
                {
                    return Results.BadRequest("El stock debe ser un valor mayor o igual a 0.");
                }
                else
                {
                    var Producto = await repo.CreateAsync(req);
                    return Results.Ok(Producto);
                }
            })
            .WithTags("Producto");

            // PUT
            app.MapPut("/Producto/{id}", async (int id, UpdateProductoRequest req,  ProductoRepository repo) =>
            {
                var updated = await repo.UpdateAsync(id, req);
                return updated ? Results.Ok() : Results.NotFound();
            })
            .WithTags("Producto");

            // DELETE
            app.MapDelete("/Producto/{id}", async (int id,  ProductoRepository repo) =>
            {
                var deleted = await repo.DeleteAsync(id);
                return deleted ? Results.Ok() : Results.NotFound();
            })
            .WithTags("Producto");
        }
    }
   
    
}