using Cart.Api.Models;
using Cart.Api.Extensions;
using Cart.Api.Endpoints;

namespace Cart.Api.Controllers;

// ALUMNOS: exposicion de todos los endpoints
public static class EndpointsExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        // ALUMNOS: se agregan  
        app.MapCartEndpoints();

    }
}