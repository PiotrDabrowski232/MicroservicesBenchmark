using MediatR;

namespace InventoryService.Application.Queries;

public record ProductDto(string Id, string Name, double Price, string Description);

public record GetProductsBenchmarkQuery(int Count) : IRequest<List<ProductDto>>;

public class GetProductsBenchmarkQueryHandler : IRequestHandler<GetProductsBenchmarkQuery, List<ProductDto>>
{
    private const string BenchmarkDescription =
        "To jest dlugi opis produktu symulujacy prawdziwe obciazenie sieciowe w systemach e-commerce. " +
        "Ma on duzo znakow, aby zbadac koszt serializacji, transferu i deserializacji danych pomiedzy mikroserwisami.";

    public Task<List<ProductDto>> Handle(GetProductsBenchmarkQuery request, CancellationToken cancellationToken)
    {
        var products = new List<ProductDto>(request.Count);

        for (int i = 0; i < request.Count; i++)
        {
            products.Add(new ProductDto(
                $"benchmark-product-{i:D6}",
                $"Benchmark Product {i}",
                19.99 + (i % 100),
                BenchmarkDescription
            ));
        }

        return Task.FromResult(products);
    }
}
