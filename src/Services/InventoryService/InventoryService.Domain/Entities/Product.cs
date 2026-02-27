namespace InventoryService.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}
