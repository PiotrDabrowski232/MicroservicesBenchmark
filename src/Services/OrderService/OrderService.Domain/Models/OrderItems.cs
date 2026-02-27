using OrderService.Domain.Abstractions;

namespace OrderService.Domain.Models
{
    public class OrderItems : IEntity
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public virtual Order? Order { get; set; }
        public Guid ProductId { get; set; }
        public virtual Product? Product { get; set; }
        public int Quantity { get; set; }
    }
}
