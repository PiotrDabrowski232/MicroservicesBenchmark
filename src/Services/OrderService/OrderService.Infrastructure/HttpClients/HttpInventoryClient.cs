using System.Net.Http.Json;

using OrderService.Application.DTOs;
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
        var payload = new { ProductId = productId, Quantity = quantity };
        var response = await _httpClient.PostAsJsonAsync("api/inventory/reserve", payload);

        return response.IsSuccessStatusCode;
    }

    public async Task<List<BenchmarkProductDto>> GetProductsBenchmarkAsync(int count)
    {
        var response = await _httpClient.GetFromJsonAsync<List<BenchmarkProductDto>>($"api/inventory/benchmark/{count}");
        return response ?? new List<BenchmarkProductDto>();
    }
}
