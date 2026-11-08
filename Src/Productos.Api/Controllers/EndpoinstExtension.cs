using Productos.Api.Models;


namespace Productos.Api.Controllers;

// ALUMNOS: exposicion de todos los endpoints
public static class EndpointsExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        // ALUMNOS: se agregan  
        
        app.MapProductosEndpoints();
    }
}