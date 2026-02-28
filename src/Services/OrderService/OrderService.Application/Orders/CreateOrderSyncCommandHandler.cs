using MediatR;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Application.Orders.Commands;

public class CreateOrderSyncCommandHandler : IRequestHandler<CreateOrderSyncCommand, Guid>
{
    private readonly IInventoryClient _inventoryClient;
    private readonly IPaymentClient _paymentClient;
    private readonly IOrderRepository _orderRepository;

    public CreateOrderSyncCommandHandler(
        IInventoryClient inventoryClient,
        IPaymentClient paymentClient,
        IOrderRepository orderRepository)
    {
        _inventoryClient = inventoryClient;
        _paymentClient = paymentClient;
        _orderRepository = orderRepository;
    }

    public async Task<Guid> Handle(CreateOrderSyncCommand request, CancellationToken cancellationToken)
    {
        var orderId = Guid.NewGuid();
        var totalAmount = 99.99m;

        var isReserved = await _inventoryClient.ReserveProductAsync(request.ProductId, request.Quantity);
        if (!isReserved)
        {
            throw new InvalidOperationException($"Nie udało się zarezerwować produktu {request.ProductId} w magazynie.");
        }

        var isPaid = await _paymentClient.ChargeAsync(orderId, totalAmount);
        if (!isPaid)
        {
            throw new InvalidOperationException($"Płatność dla zamówienia {orderId} została odrzucona.");
        }

        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            TotalAmount = totalAmount,
            CreatedAt = DateTime.UtcNow,
            Status = "Completed_Sync_REST"
        };

        await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
