using Orders.Api.Models;
using Orders.Api.Extensions;
using Orders.Api.Endpoints;

namespace Orders.Api.Controllers;

// ALUMNOS: exposicion de todos los endpoints
public static class EndpointsExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        // ALUMNOS: se agregan  
        app.MapOrdersEndpoints();

    }
}