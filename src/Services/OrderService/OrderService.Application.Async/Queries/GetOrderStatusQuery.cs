using MediatR;

using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;

namespace OrderService.Application.Async.Queries
{
    public record GetOrderStatusQuery(Guid OrderId) : IRequest<OrderDto?>;

    public class GetOrderStatusQueryHandler : IRequestHandler<GetOrderStatusQuery, OrderDto?>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderStatusQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderDto?> Handle(GetOrderStatusQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetOrderAsync(request.OrderId, cancellationToken);

            if (order == null)
            {
                return null;
            }

            return new OrderDto(order.Id, order.Status);
        }
    }
}
