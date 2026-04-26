using Messaging.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Messaging.Generics
{
    public abstract class GenericBackgroundService<TMessage> : BackgroundService
    {
        private readonly IMessageBus _messageBus;
        private readonly IServiceScopeFactory _scopeFactory;

        protected GenericBackgroundService(
            IMessageBus messageBus,
            IServiceScopeFactory scopeFactory)
        {
            _messageBus = messageBus;
            _scopeFactory = scopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _messageBus.StartConsumingAsync<TMessage>(
                async message =>
                {
                    using var scope = _scopeFactory.CreateScope();

                    await HandleMessage(message, scope, stoppingToken);
                },
                stoppingToken);
        }
        protected abstract Task HandleMessage(
        TMessage message,
        IServiceScope scope,
        CancellationToken ct);

    }
}
