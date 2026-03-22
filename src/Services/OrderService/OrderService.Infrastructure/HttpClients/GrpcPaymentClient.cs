using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Protos;

namespace OrderService.Infrastructure.HttpClients;

public class GrpcPaymentClient : IPaymentClient
{
    private readonly Payment.PaymentClient _client;

    public GrpcPaymentClient(Payment.PaymentClient client)
    {
        _client = client;
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
