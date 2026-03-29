using InventoryService.Application.Commands;
using InventoryService.Application.Queries;

using MediatR;

using Microsoft.AspNetCore.Mvc;

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

    [HttpGet("benchmark/{count}")]
    public async Task<IActionResult> GetProductsBenchmark(int count)
    {
        var query = new GetProductsBenchmarkQuery(count);
        var products = await _mediator.Send(query);
        return Ok(products);
    }
}

public record ReserveRequest(Guid ProductId, int Quantity);
