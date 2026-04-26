using MediatR;

using Microsoft.AspNetCore.Mvc;

using OrderService.Application.Async.Commands;
using OrderService.Application.Orders.Commands;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("sync")] // Ścieżka: POST /api/orders/sync
    public async Task<IActionResult> CreateOrderSync([FromBody] CreateOrderSyncCommand command)
    {
        try
        {
            var orderId = await _mediator.Send(command);

            return Ok(new { OrderId = orderId, Status = "Sukces" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Wystąpił nieoczekiwany błąd serwera." });
        }
    }

    [HttpPost("async")] // Ścieżka: POST /api/orders/async
    public async Task<IActionResult> CreateOrderAsync([FromBody] CreateOrderAsyncCommand command)
    {
        try
        {
            var order = await _mediator.Send(command);

            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Wystąpił nieoczekiwany błąd serwera." });
        }
    }
}
