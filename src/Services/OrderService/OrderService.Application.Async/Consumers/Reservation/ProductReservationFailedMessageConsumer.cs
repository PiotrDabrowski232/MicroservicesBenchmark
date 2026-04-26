using Messaging.Generics;
using Messaging.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using SharedKernel.Models;

namespace OrderService.Application.Async.Consumers.Reservation
{
    public class ProductReservationFailedMessageConsumer : GenericBackgroundService<ProductReservationFailedMessage>
    {
        public ProductReservationFailedMessageConsumer(IMessageBus messageBus, IServiceScopeFactory scopeFactory) : base(messageBus, scopeFactory)
        {
        }

        protected override Task HandleMessage(ProductReservationFailedMessage message, IServiceScope scope, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
