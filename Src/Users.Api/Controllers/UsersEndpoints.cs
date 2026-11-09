using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Users.Api.Data;
using Users.Api.DTOs;
using Users.Api.Models;
using Users.Api.Services;
using Users.Api.Exceptions;

namespace Users.Api.Controllers
{
    public static class UsersEndpoints
    {
        public static void MapUsersEndpoints(this WebApplication app)
        {
            // GET /User/{id}
            app.MapGet("/User/{id:guid}", async (Guid id, UserRepository repo) =>
            {
                var user = await repo.GetByIdAsync(id);
                if (user is null) return Results.NotFound();

                var response = new UserResponseDto(
                    Guid.Parse(user.Id.ToString()),
                    user.Nombre,
                    user.Apellido,
                    user.Email,
                    user.FechaRegistro
                );

                return Results.Ok(response);
            })
            .WithTags("User");

            // POST /Users
            app.MapPost("/Users", async (CreateUserRequest req, UserRepository repo) =>
            {
                // CAMBIO ACÁ: Según la tabla, campos faltantes o incorrectos es USR-002
                if (string.IsNullOrWhiteSpace(req.Nombre))
                    throw new BusinessRuleException("USR-002", "El campo Nombre es requerido.");
                if (string.IsNullOrWhiteSpace(req.Apellido))
                    throw new BusinessRuleException("USR-002", "El campo Apellido es requerido.");
                if (string.IsNullOrWhiteSpace(req.Email))
                    throw new BusinessRuleException("USR-002", "El campo Email es requerido.");
                if (string.IsNullOrWhiteSpace(req.PasswordHash))
                    throw new BusinessRuleException("USR-002", "La contraseña es requerida.");

                var emailChecker = new EmailAddressAttribute();
                if (!emailChecker.IsValid(req.Email))
                    throw new BusinessRuleException("USR-002", "El formato del correo electrónico no es válido.");

                var usuarioExistente = await repo.GetByEmailAsync(req.Email);
                if (usuarioExistente is not null)
                {
                    
                    throw new BusinessRuleException("USR-001", "El email ya está registrado.");
                }

                var user = await repo.CreateAsync(req);

                var responseDto = new UserResponseDto(
                    Guid.Parse(user.Id.ToString()),
                    user.Nombre,
                    user.Apellido,
                    user.Email,
                    user.FechaRegistro
                );

                return Results.Created($"/User/{responseDto.Id}", responseDto);
            })
            .WithTags("Users");

            // POST /api/users/login
            app.MapPost("/api/users/login", async (LogInUserRequest req, UserService userService) =>
            {
                var userResponse = await userService.LogInAsync(req);
                return Results.Ok(userResponse);
            })
            .WithTags("Users");
        }
    }
}