using MediatR;

using Messaging.Generics;
using Messaging.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OrderService.Application.Async.Commands;

using SharedKernel.Models;

namespace OrderService.Application.Async.Consumers.Payment
{
    public class PaymentProcessedMessageConsumer : GenericBackgroundService<PaymentProcessedMessage>
    {
        public PaymentProcessedMessageConsumer(IMessageBus messageBus, IServiceScopeFactory scopeFactory) : base(messageBus, scopeFactory)
        {
        }

        protected override async Task HandleMessage(PaymentProcessedMessage message, IServiceScope scope, CancellationToken ct)
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new MarkOrderAsPaymentProcessedCommand(message.OrderId), ct);
        }
    }
}
