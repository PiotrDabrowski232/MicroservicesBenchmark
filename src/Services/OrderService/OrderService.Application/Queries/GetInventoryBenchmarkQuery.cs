using MediatR;

using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;

namespace OrderService.Application.Queries;

public record GetInventoryBenchmarkQuery(int Count) : IRequest<List<BenchmarkProductDto>>;

public class GetInventoryBenchmarkQueryHandler : IRequestHandler<GetInventoryBenchmarkQuery, List<BenchmarkProductDto>>
{
    private readonly IInventoryClient _inventoryClient;

    public GetInventoryBenchmarkQueryHandler(IInventoryClient inventoryClient)
    {
        _inventoryClient = inventoryClient;
    }

    public async Task<List<BenchmarkProductDto>> Handle(GetInventoryBenchmarkQuery request, CancellationToken cancellationToken)
    {
        return await _inventoryClient.GetProductsBenchmarkAsync(request.Count);
    }
}
