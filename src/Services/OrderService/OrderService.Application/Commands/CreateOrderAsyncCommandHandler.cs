using MediatR;

using Messaging.Interfaces;

using Microsoft.Extensions.Options;

using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Orders.Commands;
using OrderService.Domain.Entities;

using SharedKernel.Enums;
using SharedKernel.Models;

namespace OrderService.Application.Commands
{
    public record CreateOrderAsyncCommand(Guid ProductId, int Quantity) : IRequest<OrderDto>;

    public class CreateOrderAsyncCommandHandler : IRequestHandler<CreateOrderAsyncCommand, OrderDto>
    {
        private readonly IMessageBus _messageBus;
        private readonly IOrderRepository _orderRepository;
        public CreateOrderAsyncCommandHandler(IMessageBus messageBus, IOrderRepository orderRepository)
        {
            _messageBus = messageBus;
            _orderRepository = orderRepository;
        }

        public async Task<OrderDto> Handle(CreateOrderAsyncCommand request, CancellationToken cancellationToken)
        {
            var correlationId = Guid.NewGuid();
            var totalAmount = 99.99m;

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                TotalAmount = totalAmount,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending.ToString()
            };

            await _orderRepository.AddAsync(order, cancellationToken);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            order.Status = OrderStatus.ReservingInventory.ToString();
            await _orderRepository.SaveChangesAsync(cancellationToken);

            await _messageBus.PublishAsync<ReserveProductMessage>(new ReserveProductMessage
            {
                    CorrelationId = correlationId,
                    OrderId = order.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
            }, cancellationToken);

            return new OrderDto(order.Id, order.Status);
        }
    }
}
