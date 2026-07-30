using SharedKernel.Enums;

namespace SharedKernel.Models
{
    public abstract class BaseMessage
    {
        public Guid CorrelationId { get; set; }
        public Guid OrderId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ReserveProductMessage : BaseMessage
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class ProcessPaymentMessage : BaseMessage
    {
        public decimal Amount { get; set; }
    }

    public class ProductReservedMessage : BaseMessage
    {
    }

    public class ProductReservationFailedMessage : BaseMessage
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class PaymentProcessedMessage : BaseMessage
    {
    }

    public class PaymentFailedMessage : BaseMessage
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class BenchmarkPayloadItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class BenchmarkPayloadMessage : BaseMessage
    {
        public List<BenchmarkPayloadItem> Products { get; set; } = new();
    }
}
