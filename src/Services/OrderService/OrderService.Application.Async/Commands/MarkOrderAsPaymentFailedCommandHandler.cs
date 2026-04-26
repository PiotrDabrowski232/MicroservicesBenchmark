using MediatR;

using OrderService.Application.Interfaces;

using SharedKernel.Enums;

namespace OrderService.Application.Async.Commands
{
    public record MarkOrderAsPaymentFailedCommand(Guid OrderId) : IRequest;

    public class MarkOrderAsPaymentFailedCommandHandler : IRequestHandler<MarkOrderAsPaymentFailedCommand>
    {
        private readonly IOrderRepository _orderRepository;
        public MarkOrderAsPaymentFailedCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        public async Task Handle(MarkOrderAsPaymentFailedCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetOrderAsync(request.OrderId, cancellationToken);
            order.Status = OrderStatus.PaymentFailed.ToString();
            await _orderRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
