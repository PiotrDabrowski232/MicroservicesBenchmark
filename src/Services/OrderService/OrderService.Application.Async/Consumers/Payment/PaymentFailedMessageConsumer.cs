using MediatR;

using Messaging.Generics;
using Messaging.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using OrderService.Application.Async.Commands;

using SharedKernel.Models;

namespace OrderService.Application.Async.Consumers.Payment
{
    public class PaymentFailedMessageConsumer : GenericBackgroundService<PaymentFailedMessage>
    {
        public PaymentFailedMessageConsumer(IMessageBus messageBus, IServiceScopeFactory scopeFactory) : base(messageBus, scopeFactory)
        {
        }

        protected override async Task HandleMessage(PaymentFailedMessage message, IServiceScope scope, CancellationToken ct)
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new MarkOrderAsPaymentFailedCommand(message.OrderId), ct);
        }
    }
}
