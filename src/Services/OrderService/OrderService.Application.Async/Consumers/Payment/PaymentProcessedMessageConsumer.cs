using Messaging.Generics;
using Messaging.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SharedKernel.Models;

namespace OrderService.Application.Async.Consumers.Payment
{
    public class PaymentProcessedMessageConsumer : GenericBackgroundService<PaymentProcessedMessage>
    {
        public PaymentProcessedMessageConsumer(IMessageBus messageBus, IServiceScopeFactory scopeFactory) : base(messageBus, scopeFactory)
        {
        }

        protected override Task HandleMessage(PaymentProcessedMessage message, IServiceScope scope, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
