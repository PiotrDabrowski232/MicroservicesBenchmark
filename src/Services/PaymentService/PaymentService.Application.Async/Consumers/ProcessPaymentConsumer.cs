using MediatR;

using Messaging.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PaymentService.Application.Async.Commands;

using SharedKernel.Models;

namespace PaymentService.Application.Async.Consumers
{
    public class ProcessPaymentConsumer : BackgroundService 
    {
        private readonly IMessageBus _messageBus;
        private readonly IServiceScopeFactory _scopeFactory;

        public ProcessPaymentConsumer(
            IMessageBus messageBus,
            IServiceScopeFactory scopeFactory)
        {
            _messageBus = messageBus;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _messageBus.StartConsumingAsync<ProcessPaymentMessage>(
                async message =>
                {
                    using var scope = _scopeFactory.CreateScope();

                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    await mediator.Send(new ChargeAsyncCommand(message.OrderId, message.Amount, message.CorrelationId), stoppingToken);
                },
                stoppingToken);
        }
    }
}
