using OrderService.Domain.Abstractions;

namespace OrderService.Domain.Models
{
    public class Product : IEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
