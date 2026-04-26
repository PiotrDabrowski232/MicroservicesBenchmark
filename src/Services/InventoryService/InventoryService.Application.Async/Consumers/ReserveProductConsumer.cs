using InventoryService.Application.Async.Commands;

using MediatR;

using Messaging.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SharedKernel.Models;

namespace InventoryService.Application.Async.Consumers
{
    public class ReserveProductConsumer : BackgroundService
    {
        private readonly IMessageBus _messageBus;
        private readonly IServiceScopeFactory _scopeFactory;

        public ReserveProductConsumer(
            IMessageBus messageBus,
            IServiceScopeFactory scopeFactory)
        {
            _messageBus = messageBus;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
                await _messageBus.StartConsumingAsync<ReserveProductMessage>(
                    async message =>
                    {
                        using var scope = _scopeFactory.CreateScope();

                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                        await mediator.Send(new ReserveProductAsyncCommand(message.ProductId, message.Quantity, new Dto.OrderDto(message.OrderId ,message.CorrelationId)), stoppingToken);
                    },
                    stoppingToken);
        }
    }
}
