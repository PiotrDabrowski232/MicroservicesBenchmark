using Messaging.Generics;
using Messaging.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using SharedKernel.Models;

namespace InventoryService.Application.Async.Consumers
{
    public class BenchmarkPayloadConsumer : GenericBackgroundService<BenchmarkPayloadMessage>
    {
        public BenchmarkPayloadConsumer(IMessageBus messageBus, IServiceScopeFactory scopeFactory)
            : base(messageBus, scopeFactory) { }

        protected override Task HandleMessage(BenchmarkPayloadMessage message, IServiceScope scope, CancellationToken ct)
        {
            double checksum = 0;

            var products = message.Products;
            for (int i = 0; i < products.Count; i++)
            {
                var product = products[i];
                checksum += product.Price + product.Id.Length + product.Name.Length + product.Description.Length;
            }

            if (double.IsNaN(checksum))
            {
                Console.WriteLine("Benchmark payload checksum was NaN.");
            }

            return Task.CompletedTask;
        }
    }
}
