using Grpc.Core;

using InventoryService.Api.Protos;
using InventoryService.Application.Commands;
using InventoryService.Application.Queries;

using MediatR;

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

    public override async Task<GetProductsResponse> GetProductsBenchmark(GetProductsRequest request, ServerCallContext context)
    {
        var query = new GetProductsBenchmarkQuery(request.Count);
        var products = await _mediator.Send(query);

        var response = new GetProductsResponse();

        foreach (var p in products)
        {
            response.Products.Add(new ProductDtoMessage
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description
            });
        }

        return response;
    }

    public override async Task<TransportPingResponse> GetTransportPing(TransportPingRequest request, ServerCallContext context)
    {
        var query = new GetTransportPingQuery();
        var result = await _mediator.Send(query);

        return new TransportPingResponse
        {
            Message = result.Message,
            Value = result.Value
        };
    }
}
