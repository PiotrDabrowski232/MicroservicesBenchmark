using MediatR;

using Microsoft.AspNetCore.Mvc;

using PaymentService.Application.Commands;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("charge")]
    public async Task<IActionResult> Charge([FromBody] ChargeRequest request)
    {
        var command = new ChargeCommand(request.OrderId, request.Amount);

        var transactionId = await _mediator.Send(command);

        return Ok(new { Success = true, TransactionId = transactionId });
    }
}

public record ChargeRequest(Guid OrderId, decimal Amount);
