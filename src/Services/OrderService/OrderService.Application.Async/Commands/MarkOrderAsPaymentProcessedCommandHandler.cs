using MediatR;

using OrderService.Application.Interfaces;

using SharedKernel.Enums;

namespace OrderService.Application.Async.Commands
{
    public record MarkOrderAsPaymentProcessedCommand(Guid OrderId) : IRequest;

    public class MarkOrderAsPaymentProcessedCommandHandler : IRequestHandler<MarkOrderAsPaymentProcessedCommand>
    {
        private readonly IOrderRepository _orderRepository;
        public MarkOrderAsPaymentProcessedCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        public async Task Handle(MarkOrderAsPaymentProcessedCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetOrderAsync(request.OrderId, cancellationToken);
            order.Status = OrderStatus.Completed.ToString();
            await _orderRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
