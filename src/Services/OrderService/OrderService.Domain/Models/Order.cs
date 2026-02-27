using OrderService.Domain.Abstractions;
using OrderService.Domain.Enum;

namespace OrderService.Domain.Models
{
    public class Order : IEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public virtual ICollection<OrderItems> OrderItems { get; set; }
        public OrderStatus Status { get; set; }
    }
}
