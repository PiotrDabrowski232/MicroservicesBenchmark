using MediatR;

using Messaging.Generics;
using Messaging.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PaymentService.Application.Async.Commands;

using SharedKernel.Models;

namespace PaymentService.Application.Async.Consumers
{
    public class ProcessPaymentConsumer : GenericBackgroundService<ProcessPaymentMessage> 
    {

        public ProcessPaymentConsumer(
            IMessageBus messageBus,
            IServiceScopeFactory scopeFactory) : base(messageBus, scopeFactory) { }

        protected async override Task HandleMessage(ProcessPaymentMessage message, IServiceScope scope, CancellationToken ct)
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new ChargeAsyncCommand(message.OrderId, message.Amount, message.CorrelationId), ct);
        }
    }
}
