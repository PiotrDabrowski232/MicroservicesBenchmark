using MediatR;

using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;

namespace OrderService.Application.Queries;

public record GetTransportPingQuery() : IRequest<TransportPingDto>;

public class GetTransportPingQueryHandler : IRequestHandler<GetTransportPingQuery, TransportPingDto>
{
    private readonly IInventoryClient _inventoryClient;

    public GetTransportPingQueryHandler(IInventoryClient inventoryClient)
    {
        _inventoryClient = inventoryClient;
    }

    public async Task<TransportPingDto> Handle(GetTransportPingQuery request, CancellationToken cancellationToken)
    {
        return await _inventoryClient.GetTransportPingAsync();
    }
}
