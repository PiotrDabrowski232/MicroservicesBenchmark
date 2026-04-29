using MediatR;

using Messaging.Generics;
using Messaging.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OrderService.Application.Async.Commands;

using SharedKernel.Models;

namespace OrderService.Application.Async.Consumers.Reservation
{
    public class ProductReservedMessageConsumer : GenericBackgroundService<ProductReservedMessage>
    {
        public ProductReservedMessageConsumer(IMessageBus messageBus, IServiceScopeFactory scopeFactory) : base(messageBus, scopeFactory) { }

        protected override async Task HandleMessage(ProductReservedMessage message, IServiceScope scope, CancellationToken ct)
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new MarkOrderAsInventoryReservedCommand(message.OrderId, message.CorrelationId), ct);
        }
    }
}
