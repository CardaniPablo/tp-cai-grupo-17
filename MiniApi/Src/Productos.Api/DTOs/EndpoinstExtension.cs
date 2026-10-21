using Productos.Api.Extensions.Endpoints;
using Productos.Api.Models;

namespace Productos.Api.Extensions;

// ALUMNOS: exposicion de todos los endpoints
public static class EndpointsExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        // ALUMNOS: se agregan  
        app.MapItemEndpoints();
    }
}