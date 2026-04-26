using MediatR;

using OrderService.Application.Interfaces;

using SharedKernel.Enums;

namespace OrderService.Application.Async.Commands
{
    public record MarkOrderAsInventoryFailedCommand(Guid OrderId) : IRequest;

    public class MarkOrderAsInventoryFailedCommandHandler : IRequestHandler<MarkOrderAsInventoryFailedCommand>
    {
        private readonly IOrderRepository _orderRepository;

        public MarkOrderAsInventoryFailedCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Handle(MarkOrderAsInventoryFailedCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetOrderAsync(request.OrderId, cancellationToken);

            order.Status = OrderStatus.InventoryFailed.ToString();

            await _orderRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
