using MediatR;

namespace InventoryService.Application.Queries;

public record TransportPingDto(string Message, int Value);

public record GetTransportPingQuery() : IRequest<TransportPingDto>;

public class GetTransportPingQueryHandler : IRequestHandler<GetTransportPingQuery, TransportPingDto>
{
    public Task<TransportPingDto> Handle(GetTransportPingQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new TransportPingDto("pong", 1));
    }
}
