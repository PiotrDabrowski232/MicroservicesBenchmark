using MediatR;

namespace InventoryService.Application.Queries;

public record ProductDto(string Id, string Name, double Price, string Description);

public record GetProductsBenchmarkQuery(int Count) : IRequest<List<ProductDto>>;

public class GetProductsBenchmarkQueryHandler : IRequestHandler<GetProductsBenchmarkQuery, List<ProductDto>>
{
    public Task<List<ProductDto>> Handle(GetProductsBenchmarkQuery request, CancellationToken cancellationToken)
    {
        var products = new List<ProductDto>(request.Count);

        for (int i = 0; i < request.Count; i++)
        {
            products.Add(new ProductDto(
                Guid.NewGuid().ToString(),
                $"Benchmark Product {i}",
                19.99 + (i % 100),
                "To jest długi opis produktu symulujący prawdziwe obciążenie sieciowe w systemach e-commerce.Ma on bardzo dużo znaków, w celu zbadania jak szybko Kestrel pakuje dane, jak szybko lecą one przez sieć i jak szybko klient je rozpakowywuje."
            ));
        }

        return Task.FromResult(products);
    }
}
