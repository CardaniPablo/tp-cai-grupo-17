using Orders.Api.DTOs;
namespace Orders.Api.Services
{
    public class ProductClient : IProductClient
    {
        private readonly HttpClient _httpClient;

        public ProductClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProductDto?> GetByIdAsync(Guid IdProducto)
        {
            return await _httpClient
                .GetFromJsonAsync<ProductDto>(
                    $"api/products/{IdProducto}");
        }
    }
}
