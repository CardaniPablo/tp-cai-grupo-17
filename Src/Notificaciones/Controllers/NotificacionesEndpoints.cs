using Microsoft.AspNetCore.Mvc;
using Notificaciones.Api.DTOs;
using Notificaciones.Api.Services;

namespace Notificaciones.Api.Endpoints;

public static class NotificacionesEndpoints
{
    public static void MapNotificationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/notifications");

        group.MapPost("/send", async ([FromBody] CreateNotificacionesRequest request, NotificationService service) =>
        {
            var response = await service.SendNotificationAsync(request);
            return Results.Json(response, statusCode: 201);
        });

        group.MapGet("/{userId:guid}", async ([FromRoute] Guid userId, NotificationService service) =>
        {
            var response = await service.GetNotificationsByUserIdAsync(userId);
            return Results.Ok(response);
        });
    }
}