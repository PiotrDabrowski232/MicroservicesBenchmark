using MediatR;

using Messaging.Interfaces;

using OrderService.Application.Interfaces;

using SharedKernel.Enums;
using SharedKernel.Models;

namespace OrderService.Application.Async.Commands
{
    public record MarkOrderAsInventoryReservedCommand(Guid OrderId, Guid CorrelationId) : IRequest;

    public class MarkOrderAsInventoryReservedCommandHandler : IRequestHandler<MarkOrderAsInventoryReservedCommand>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMessageBus _messageBus;

        public MarkOrderAsInventoryReservedCommandHandler(IOrderRepository orderRepository, IMessageBus messageBus)
        {
            _orderRepository = orderRepository;
            _messageBus = messageBus;
        }

        public async Task Handle(MarkOrderAsInventoryReservedCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetOrderAsync(request.OrderId, cancellationToken);

            order.Status = OrderStatus.InventoryReserved.ToString();
            await _orderRepository.SaveChangesAsync(cancellationToken);

            await _messageBus.PublishAsync<ProcessPaymentMessage>(
                new ProcessPaymentMessage
                {
                    OrderId = order.Id,
                    Amount = order.TotalAmount,
                    CorrelationId = request.CorrelationId
                }, cancellationToken);

            order.Status = OrderStatus.ProcessingPayment.ToString();
            await _orderRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
