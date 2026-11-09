using Users.Api.Extensions;
using Users.Api.Models;

namespace Users.Api.Controllers;

// ALUMNOS: exposicion de todos los endpoints
public static class EndpointsExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        // ALUMNOS: se agregan  
        
        app.MapUsersEndpoints();
    }
}