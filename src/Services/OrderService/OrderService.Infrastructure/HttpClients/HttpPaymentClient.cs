using System.Net.Http.Json;

using OrderService.Application.Interfaces;

namespace OrderService.Infrastructure.HttpClients;

public class HttpPaymentClient : IPaymentClient
{
    private readonly HttpClient _httpClient;

    public HttpPaymentClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> ChargeAsync(Guid orderId, decimal amount)
    {
        var requestData = new { OrderId = orderId, Amount = amount };
        var response = await _httpClient.PostAsJsonAsync("/api/payment/charge", requestData);

        return response.IsSuccessStatusCode;
    }
}
