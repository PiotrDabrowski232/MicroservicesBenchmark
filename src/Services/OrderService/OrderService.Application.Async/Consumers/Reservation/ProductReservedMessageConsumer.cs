using Messaging.Generics;
using Messaging.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SharedKernel.Models;

namespace OrderService.Application.Async.Consumers.Reservation
{
    public class ProductReservedMessageConsumer : GenericBackgroundService<ProductReservedMessage>
    {
        public ProductReservedMessageConsumer(IMessageBus messageBus, IServiceScopeFactory scopeFactory) : base(messageBus, scopeFactory) { }

        protected override Task HandleMessage(ProductReservedMessage message, IServiceScope scope, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
