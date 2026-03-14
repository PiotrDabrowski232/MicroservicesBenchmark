using MediatR;
using Microsoft.AspNetCore.Mvc;
using InventoryService.Application.Commands;

namespace InventoryService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveProduct([FromBody] ReserveRequest request)
    {
        var command = new ReserveProductCommand(request.ProductId, request.Quantity);

        var isSuccess = await _mediator.Send(command);

        if (!isSuccess)
        {
            return BadRequest(new { Error = "Failed to reserve inventory." });
        }

        return Ok(new { Success = true });
    }
}

public record ReserveRequest(Guid ProductId, int Quantity);
