namespace OrderService.Application.Interfaces;

public interface IPaymentClient
{
    Task<bool> ChargeAsync(Guid orderId, decimal amount);
}
