using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InventoryService.Infrastructure.Data;

public static class InventoryDbContextSeed
{
    public static async Task SeedAsync(InventoryDbContext context, ILogger logger)
    {
        if (!context.Products.Any())
        {
            var products = new List<Product>
            {
                new Product { Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), Name = "Test Product 1", StockQuantity = 100000 },
                new Product { Id = Guid.NewGuid(), Name = "Test Product 2", StockQuantity = 50000 },
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}
