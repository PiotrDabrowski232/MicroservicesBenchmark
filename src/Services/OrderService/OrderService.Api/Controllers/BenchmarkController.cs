using MediatR;

using Microsoft.AspNetCore.Mvc;

using OrderService.Application.Queries;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BenchmarkController : ControllerBase
{
    private readonly IMediator _mediator;

    public BenchmarkController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("data-transfer/{count}")]
    public async Task<IActionResult> TestDataTransfer(int count)
    {
        var query = new GetInventoryBenchmarkQuery(count);
        var result = await _mediator.Send(query);

        return Ok(new { TotalReceived = result.Count });
    }

    [HttpGet("transport-ping")]
    public async Task<IActionResult> TransportPing()
    {
        var query = new GetTransportPingQuery();
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}
