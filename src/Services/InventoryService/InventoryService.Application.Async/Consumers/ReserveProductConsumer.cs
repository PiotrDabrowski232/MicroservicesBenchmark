using InventoryService.Application.Async.Commands;
using InventoryService.Application.Async.Dto;

using MediatR;

using Messaging.Generics;
using Messaging.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using SharedKernel.Models;

namespace InventoryService.Application.Async.Consumers
{
    public class ReserveProductConsumer : GenericBackgroundService<ReserveProductMessage>
    {

        public ReserveProductConsumer(IMessageBus messageBus, IServiceScopeFactory scopeFactory) : base(messageBus, scopeFactory) { }

        protected async override Task HandleMessage(ReserveProductMessage message, IServiceScope scope, CancellationToken ct)
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.Send(
                new ReserveProductAsyncCommand(
                    message.ProductId,
                    message.Quantity,
                    new OrderDto(message.OrderId, message.CorrelationId)),
                ct);
        }
    }
}
