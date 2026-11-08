using Notificaciones.Api.Data;
using Notificaciones.Api.DTOs;
using Notificaciones.Api.Exceptions;
using Notificaciones.Api.Models;

namespace Notificaciones.Api.Services;

public class NotificationService
{
    private readonly NotificationsRepository _repository;
    private readonly IHttpClientFactory _httpFactory;

    public NotificationService(NotificationsRepository repository, IHttpClientFactory httpFactory)
    {
        _repository = repository;
        _httpFactory = httpFactory;
    }

    public async Task<NotificacionesResponse> SendNotificationAsync(CreateNotificacionesRequest request)
        => MapToResponse(await ValidarYConstruirNotificacionAsync(request));

    public async Task<IEnumerable<NotificacionesResponse>> GetNotificationsByUserIdAsync(Guid userId)
        => (await _repository.GetByUserIdAsync(userId) is { } list && list.Any())
            ? list.Select(MapToResponse)
            : throw new NotFoundException("NTF-003", "No se encontraron notificaciones para el usuario.");

    private async Task<Notification> ValidarYConstruirNotificacionAsync(CreateNotificacionesRequest request)
    {
        // 1. Validar NTF-002: Campos obligatorios o vacíos
        if (string.IsNullOrWhiteSpace(request.Mensaje) || string.IsNullOrWhiteSpace(request.Tipo) || request.UsuarioId == Guid.Empty)
            throw new BusinessRuleException("NTF-002", "Los datos de la notificación son inválidos o contienen campos vacíos.");

        // 2. Validar NTF-002: Tipos permitidos por el enunciado del TP
        if (request.Tipo is not ("Email" or "Push" or "SMS"))
            throw new BusinessRuleException("NTF-002", $"El tipo de notificación '{request.Tipo}' no es reconocido. Tipos válidos: Email, Push, SMS.");

        // 3. Validar NTF-001: Comunicación síncrona remota (Ajustado a BusinessRuleException)
        if (!await ExisteUsuarioEnMicroservicio(request.UsuarioId))
            throw new BusinessRuleException("NTF-001", "Usuario no encontrado.");

        var nuevaNotificacion = new Notification
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId,
            Mensaje = request.Mensaje,
            Tipo = request.Tipo,
            Estado = "Enviada",
            FechaEnvio = DateTime.UtcNow
        };

        await _repository.CreateAsync(nuevaNotificacion);
        return nuevaNotificacion;
    }

    private async Task<bool> ExisteUsuarioEnMicroservicio(Guid usuarioId)
    {
        try
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            using var client = new HttpClient(handler);

            var response = await client.GetAsync($"https://localhost:7067/User/{usuarioId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch
        {
            throw;
        }
    }

    private static NotificacionesResponse MapToResponse(Notification n)
        => new(n.Id, n.UsuarioId, n.Mensaje, n.Tipo, n.Estado, n.FechaEnvio);
}