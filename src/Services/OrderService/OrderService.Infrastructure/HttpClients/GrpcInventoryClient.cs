using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Protos;

namespace OrderService.Infrastructure.HttpClients;

public class GrpcInventoryClient : IInventoryClient
{
    private readonly Inventory.InventoryClient _client;

    public GrpcInventoryClient(Inventory.InventoryClient client)
    {
        _client = client;
    }

    public async Task<bool> ReserveProductAsync(Guid productId, int quantity)
    {
        var request = new ReserveRequestMessage
        {
            ProductId = productId.ToString(),
            Quantity = quantity
        };

        var response = await _client.ReserveProductAsync(request);
        return response.Success;
    }

    public async Task<List<BenchmarkProductDto>> GetProductsBenchmarkAsync(int count)
    {
        var request = new GetProductsRequest { Count = count };
        var response = await _client.GetProductsBenchmarkAsync(request);

        var result = new List<BenchmarkProductDto>(response.Products.Count);

        foreach (var p in response.Products)
        {
            result.Add(new BenchmarkProductDto(p.Id, p.Name, p.Price, p.Description));
        }

        return result;
    }

    public async Task<TransportPingDto> GetTransportPingAsync()
    {
        var response = await _client.GetTransportPingAsync(new TransportPingRequest());

        return new TransportPingDto(response.Message, response.Value);
    }
}
