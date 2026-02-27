using InventoryService.Infrastructure.Data;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly InventoryDbContext _dbContext;

    public InventoryController(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveProduct([FromBody] ReserveRequest request)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId);

        if (product == null)
        {
            return BadRequest(new { Error = "ProductNotFound" });
        }

        if (product.StockQuantity < request.Quantity)
        {
            return BadRequest(new { Error = "OutOfStock" });
        }

        product.StockQuantity -= request.Quantity;
        await _dbContext.SaveChangesAsync();

        return Ok(new { Success = true });
    }
}

public record ReserveRequest(Guid ProductId, int Quantity);
