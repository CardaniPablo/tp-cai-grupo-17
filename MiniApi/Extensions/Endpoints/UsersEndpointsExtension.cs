using k8s.KubeConfigModels;
using Microsoft.AspNetCore.Mvc;
using MiniApi.Data;
using MiniApi.Models;

namespace MiniApi.Extensions.Endpoints

{
    // ALUMNOS: clase donde se definen los endpoints para cada entidad

    public static class UsersEndpointsExtension
    {
        public static void MapUsersEndpoints(this WebApplication app)
        {
            
            // POST
            app.MapPost("/Users", async (CreateUserRequest req, UserRepository repo) =>
            {
                var User = await repo.CreateAsync(req);
                return Results.Ok(User);
            })
            .WithTags("Users");

        }

    }
}