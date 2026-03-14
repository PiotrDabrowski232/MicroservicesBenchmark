using Grpc.Net.Client;

using Microsoft.Extensions.Configuration;

using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Protos;

namespace OrderService.Infrastructure.HttpClients;

public class GrpcPaymentClient : IPaymentClient
{
    private readonly Payment.PaymentClient _client;

    public GrpcPaymentClient(IConfiguration configuration)
    {
        var address = configuration["GrpcUrls:InventoryService"] ?? "http://payment-service:8080";

        var httpHandler = new SocketsHttpHandler
        {
            EnableMultipleHttp2Connections = true
        };

        var channelOptions = new GrpcChannelOptions { HttpHandler = httpHandler };
        var channel = GrpcChannel.ForAddress(address, channelOptions);

        _client = new Payment.PaymentClient(channel);
    }

    public async Task<bool> ChargeAsync(Guid orderId, decimal amount)
    {
        var request = new ChargeRequestMessage
        {
            OrderId = orderId.ToString(),
            Amount = (double)amount
        };

        var response = await _client.ChargeAsync(request);

        return response.Success;
    }
}
