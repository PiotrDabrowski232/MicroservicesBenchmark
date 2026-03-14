using Grpc.Core;

using MediatR;

using PaymentService.Api.Protos;
using PaymentService.Application.Commands;

namespace PaymentService.Api.GrpcServices;

public class PaymentGrpcService : Payment.PaymentBase
{
    private readonly IMediator _mediator;

    public PaymentGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<ChargeResponseMessage> Charge(ChargeRequestMessage request, ServerCallContext context)
    {
        var command = new ChargeCommand(Guid.Parse(request.OrderId), (decimal)request.Amount);

        var transactionId = await _mediator.Send(command);

        return new ChargeResponseMessage
        {
            Success = true,
            TransactionId = transactionId
        };
    }
}
