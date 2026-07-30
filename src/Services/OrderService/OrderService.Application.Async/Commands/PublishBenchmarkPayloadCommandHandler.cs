using MediatR;

using Messaging.Interfaces;

using SharedKernel.Models;

namespace OrderService.Application.Async.Commands
{
    public record PublishBenchmarkPayloadCommand(int Count) : IRequest<int>;

    public class PublishBenchmarkPayloadCommandHandler : IRequestHandler<PublishBenchmarkPayloadCommand, int>
    {
        private const string BenchmarkDescription =
            "To jest dlugi opis produktu symulujacy prawdziwe obciazenie sieciowe w systemach e-commerce. " +
            "Ma on duzo znakow, aby zbadac koszt serializacji, transferu i deserializacji danych pomiedzy mikroserwisami.";

        private readonly IMessageBus _messageBus;

        public PublishBenchmarkPayloadCommandHandler(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        public async Task<int> Handle(PublishBenchmarkPayloadCommand request, CancellationToken cancellationToken)
        {
            var products = new List<BenchmarkPayloadItem>(request.Count);

            for (int i = 0; i < request.Count; i++)
            {
                products.Add(new BenchmarkPayloadItem
                {
                    Id = $"benchmark-product-{i:D6}",
                    Name = $"Benchmark Product {i}",
                    Price = 19.99 + (i % 100),
                    Description = BenchmarkDescription
                });
            }

            await _messageBus.PublishAsync(new BenchmarkPayloadMessage
            {
                CorrelationId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                Products = products
            }, cancellationToken);

            return request.Count;
        }
    }
}
