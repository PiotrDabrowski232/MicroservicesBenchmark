using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces;

public interface IInventoryClient
{
    Task<bool> ReserveProductAsync(Guid productId, int quantity);
    Task<List<BenchmarkProductDto>> GetProductsBenchmarkAsync(int count);
}
