using OrderService.Application.Interfaces;
using OrderService.Application.Orders.Commands;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Orchestrators
{
    public class SyncOrderWorkflowOrchestrator : IOrderWorkflowOrchestrator
    {
        private readonly IInventoryClient _inventoryClient;
        private readonly IPaymentClient _paymentClient;
        private readonly IOrderRepository _orderRepository;
        public SyncOrderWorkflowOrchestrator(
        IInventoryClient inventoryClient,
        IPaymentClient paymentClient,
        IOrderRepository orderRepository)
        {
            _inventoryClient = inventoryClient;
            _paymentClient = paymentClient;
            _orderRepository = orderRepository;
        }

        public async Task<Guid> ProcessAsync(CreateOrderSyncCommand command, CancellationToken cancellationToken)
        {

            var orderId = Guid.NewGuid();
            var totalAmount = 99.99m;

            var isReserved = await _inventoryClient.ReserveProductAsync(command.ProductId, command.Quantity);
            if (!isReserved)
            {
                throw new InvalidOperationException($"Nie udało się zarezerwować produktu {command.ProductId} w magazynie.");
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
}
