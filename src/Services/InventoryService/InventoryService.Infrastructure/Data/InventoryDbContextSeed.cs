using System.Text.Json;

using InventoryService.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryService.Infrastructure.Data;

public static class InventoryDbContextSeed
{
    public static async Task SeedAsync(InventoryDbContext context, ILogger logger)
    {
        if (await context.Products.AnyAsync())
            return;

        var path = Path.Combine(AppContext.BaseDirectory, "Seed", "Products.json");

        if (!File.Exists(path))
        {
            logger.LogWarning("Products seed file not found: {Path}", path);
            return;
        }

        var json = await File.ReadAllTextAsync(path);

        var products = JsonSerializer.Deserialize<List<Product>>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (products is null || products.Count == 0)
            return;

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} products", products.Count);
    }
}
