using k8s.KubeConfigModels;
using System.Net.Http.Json;
using Cart.Api.DTOs;
namespace Cart.Api.Services
{
    public class UserClient : IUserClient
    {
        private readonly HttpClient _httpClient;

        public UserClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<object?> GetByIdAsync(Guid UserId)
        {
            try
            {
                // Reemplaza 'object' por el DTO real de tu usuario si lo tenés (ej. UserDto)
                return await _httpClient.GetFromJsonAsync<UserDto>($"api/users/{UserId}");
            }
            catch (HttpRequestException)
            {
                return null; // Si da 404 o falla la conexión, devuelve null
            }
        }
    }
}