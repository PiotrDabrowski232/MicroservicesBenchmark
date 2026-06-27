using MediatR;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;

namespace OrderService.Application.Queries;

public record GetComplexInventoryBenchmarkQuery(int Count) : IRequest<List<BenchmarkComplexItemDto>>;

public class GetComplexInventoryBenchmarkQueryHandler : IRequestHandler<GetComplexInventoryBenchmarkQuery, List<BenchmarkComplexItemDto>>
{
    private readonly IInventoryClient _inventoryClient;

    public GetComplexInventoryBenchmarkQueryHandler(IInventoryClient inventoryClient)
    {
        _inventoryClient = inventoryClient;
    }

    public async Task<List<BenchmarkComplexItemDto>> Handle(GetComplexInventoryBenchmarkQuery request, CancellationToken cancellationToken)
    {
        return await _inventoryClient.GetComplexPayloadBenchmarkAsync(request.Count);
    }
}
