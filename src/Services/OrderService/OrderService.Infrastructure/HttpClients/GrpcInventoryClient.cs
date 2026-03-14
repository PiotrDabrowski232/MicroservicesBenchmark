using Grpc.Net.Client;

using Microsoft.Extensions.Configuration;

using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Protos;

namespace OrderService.Infrastructure.HttpClients;

public class GrpcInventoryClient : IInventoryClient
{
    private readonly Inventory.InventoryClient _client;

    public GrpcInventoryClient(IConfiguration configuration)
    {
        var address = configuration["GrpcUrls:InventoryService"] ?? "http://inventory-service:5003";

        var httpHandler = new SocketsHttpHandler
        {
            EnableMultipleHttp2Connections = true
        };

        var channelOptions = new GrpcChannelOptions { HttpHandler = httpHandler };
        var channel = GrpcChannel.ForAddress(address, channelOptions);

        _client = new Inventory.InventoryClient(channel);
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
}
