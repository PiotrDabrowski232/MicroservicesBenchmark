using System.Net.Http.Json;

using OrderService.Application.Interfaces;

namespace OrderService.Infrastructure.HttpClients;

public class HttpInventoryClient : IInventoryClient
{
    private readonly HttpClient _httpClient;

    public HttpInventoryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> ReserveProductAsync(Guid productId, int quantity)
    {
        var requestData = new { ProductId = productId, Quantity = quantity };

        var response = await _httpClient.PostAsJsonAsync("/api/inventory/reserve", requestData);

        return response.IsSuccessStatusCode;
    }
}
