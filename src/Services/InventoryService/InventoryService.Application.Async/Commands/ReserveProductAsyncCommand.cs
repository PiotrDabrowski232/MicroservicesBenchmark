using InventoryService.Application.Async.Dto;
using InventoryService.Application.Interfaces;

using MediatR;

using Messaging.Interfaces;

using SharedKernel.Models;

namespace InventoryService.Application.Async.Commands
{
    public record ReserveProductAsyncCommand(Guid ProductId, int Quantity, OrderDto Order) : IRequest<bool>;

    public class ReserveProductAsyncCommandHandler : IRequestHandler<ReserveProductAsyncCommand, bool>
    {
        private readonly IInventoryRepository _repository;
        private readonly IMessageBus _messageBus;

        public ReserveProductAsyncCommandHandler(IInventoryRepository repository, IMessageBus messageBus)
        {
            _repository = repository;
            _messageBus = messageBus;
        }

        public async Task<bool> Handle(ReserveProductAsyncCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.GetProductByIdAsync(request.ProductId, cancellationToken);

            if (product == null || product.StockQuantity < request.Quantity)
            {
                await _messageBus.PublishAsync<ProductReservationFailedMessage>(new ProductReservationFailedMessage
                {
                    CorrelationId = request.Order.CorrelationId,
                    OrderId = request.Order.OrderId,
                    Reason = "Insufficient stock"
                }, cancellationToken);

                return false;
            }

            product.StockQuantity -= request.Quantity;
            await _repository.SaveChangesAsync(cancellationToken);

            await _messageBus.PublishAsync<ProductReservedMessage>(new ProductReservedMessage
            {
                CorrelationId = request.Order.CorrelationId,
                OrderId = request.Order.OrderId
            }, cancellationToken);

            return true;
        }
    }
}
