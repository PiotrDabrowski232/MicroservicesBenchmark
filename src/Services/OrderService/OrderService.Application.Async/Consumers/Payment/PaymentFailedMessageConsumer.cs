using Messaging.Generics;
using Messaging.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using SharedKernel.Models;

namespace OrderService.Application.Async.Consumers.Payment
{
    public class PaymentFailedMessageConsumer : GenericBackgroundService<PaymentFailedMessage>
    {
        public PaymentFailedMessageConsumer(IMessageBus messageBus, IServiceScopeFactory scopeFactory) : base(messageBus, scopeFactory)
        {
        }

        protected override Task HandleMessage(PaymentFailedMessage message, IServiceScope scope, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
