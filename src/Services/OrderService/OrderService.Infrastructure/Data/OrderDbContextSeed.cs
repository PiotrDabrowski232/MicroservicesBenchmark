using Microsoft.Extensions.Logging;

using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Data;

public static class OrderDbContextSeed
{
    public static async Task SeedAsync(OrderDbContext context)
    {
        if (!context.Orders.Any())
        {

            int totalOrders = 100_000;
            int batchSize = 10_000;
            var random = new Random();
            var statuses = new[] { "Pending", "Completed", "Cancelled", "Shipped" };

            for (int i = 0; i < totalOrders; i += batchSize)
            {
                var ordersBatch = new List<Order>();

                for (int j = 0; j < batchSize; j++)
                {
                    ordersBatch.Add(new Order
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = Guid.NewGuid(),
                        TotalAmount = (decimal)(random.NextDouble() * 1000) + 10,
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 365)),
                        Status = statuses[random.Next(statuses.Length)]
                    });
                }

                await context.Orders.AddRangeAsync(ordersBatch);
                await context.SaveChangesAsync();

            }

        }
    }
}
