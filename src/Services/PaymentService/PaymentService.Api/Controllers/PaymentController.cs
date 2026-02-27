using Microsoft.AspNetCore.Mvc;

using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly PaymentDbContext _dbContext;

    public PaymentController(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("charge")]
    public async Task<IActionResult> Charge([FromBody] ChargeRequest request)
    {
        await Task.Delay(300);

        var transaction = new TransactionRecord
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            Amount = request.Amount,
            Status = "Success",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Transactions.Add(transaction);
        await _dbContext.SaveChangesAsync();

        return Ok(new { Success = true, TransactionId = transaction.Id.ToString() });
    }
}

public record ChargeRequest(Guid OrderId, decimal Amount);
