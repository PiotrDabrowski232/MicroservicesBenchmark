namespace OrderService.Application.Interfaces;

public interface IInventoryClient
{
    Task<bool> ReserveProductAsync(Guid productId, int quantity);
}
