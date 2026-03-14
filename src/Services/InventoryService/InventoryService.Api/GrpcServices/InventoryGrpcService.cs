using Grpc.Core;
using MediatR;
using InventoryService.Api.Protos;
using InventoryService.Application.Commands;

namespace InventoryService.Api.GrpcServices;

public class InventoryGrpcService : Inventory.InventoryBase
{
    private readonly IMediator _mediator;

    public InventoryGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<ReserveResponseMessage> ReserveProduct(ReserveRequestMessage request, ServerCallContext context)
    {
        var command = new ReserveProductCommand(Guid.Parse(request.ProductId), request.Quantity);

        var isSuccess = await _mediator.Send(command);

        if (!isSuccess)
        {
            return new ReserveResponseMessage { Success = false, ErrorMessage = "Failed" };
        }

        return new ReserveResponseMessage { Success = true, ErrorMessage = "" };
    }
}
