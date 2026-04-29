using MediatR;

using Messaging.Generics;
using Messaging.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using OrderService.Application.Async.Commands;

using SharedKernel.Models;

namespace OrderService.Application.Async.Consumers.Reservation
{
    public class ProductReservationFailedMessageConsumer : GenericBackgroundService<ProductReservationFailedMessage>
    {
        public ProductReservationFailedMessageConsumer(IMessageBus messageBus, IServiceScopeFactory scopeFactory) : base(messageBus, scopeFactory)
        {
        }

        protected override async Task HandleMessage(ProductReservationFailedMessage message, IServiceScope scope, CancellationToken ct)
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new MarkOrderAsInventoryFailedCommand(message.OrderId), ct);
        }
    }
}
