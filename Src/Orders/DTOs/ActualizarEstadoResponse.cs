namespace Orders.Api.DTOs
{
    public record ActualizarEstadoResponse(
        Guid Id,
        string Estado,
        DateTime FechaActualizacion
    );
}
